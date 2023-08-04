using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Services;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Writers;
using System.Globalization;
using System.Diagnostics;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly IIngredientsService _ingService;
        private readonly IUserService _userService;
        private readonly ClaimsIdentity? _claims;

        public RecipesController(IRecipeService recipeService, IIngredientsService ingService, IUserService userService, IHttpContextAccessor _context)
        {
            _recipeService = recipeService;
            _ingService = ingService;
            _userService = userService;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
        }

        [HttpPost]
        public async Task<ActionResult> PostRecipe(RecipeData recipe)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest("You must be logged in to post a recipe.");
                }
                
                Recipe r = new()
                {
                    RecipeDescription = recipe.RecipeDescription,
                    RecipeTitle = recipe.RecipeTitle,
                    CreatedDatetime = DateTime.Now,
                    UserId = currentId
                };

                for (int si = 0; si < recipe.RecipeSteps.Count; si++)
                {
                    RecipeStepData stepData = recipe.RecipeSteps.ElementAt(si);
                    RecipeStep step = new()
                    {
                        StepDescription = stepData.StepDescription,
                        StepOrder = stepData.StepOrder,
                        StepIngredients = new List<StepIngredient>(),
                        RecipeId = r.RecipeId
                    };
                    
                    for (int ii = 0; ii < stepData.StepIngredients.Count; ii++)
                    {
                        StepIngredientData ingData = stepData.StepIngredients.ElementAt(ii);

                        step.StepIngredients.Add(new()
                        {
                            IngredientId = ingData.IngredientId,
                            IngredientMeasurementUnit = ingData.IngredientMeasurementUnit,
                            IngredientMeasurementValue = ingData.IngredientMeasurementValue,
                            StepId = step.StepId
                        });
                    }
                    r.RecipeSteps.Add(step);
                }

                if (await _recipeService.CreateRecipe(r))
                {
                    if (await _recipeService.AddRecipeToCookbook(currentId, r))
                    {

                        return Ok(new { r.RecipeId });
                    }
                    else
                    {
                        return StatusCode(500, "Recipe Created but failed to add to your cookbook");
                    }
                }
                else
                {
                    return StatusCode(500, "Error creating recipe.");
                }
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpDelete("{recipeId}/dissociate")]
        public async Task<IActionResult> DissociateRecipe(int recipeId)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest("You must be logged in.");
                }

                Recipe? r = await _recipeService.GetRecipeById(recipeId);
                if (r != null)
                {
                    if (r.UserId != currentId)
                    {
                        return Unauthorized("You are not associated with this recipe");
                    }
                    r.UserId = null;
                    int numRows = await _recipeService.UpdateRecipe(r);
                    if (numRows > 0)
                    {
                        return Ok();
                    }
                    else
                    {
                        return StatusCode(500, "There was an error dissociating");
                    }
                }
                else
                {
                    return NotFound("This recipe does not exist.");
                }

            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpDelete("{recipeId}")]
        public async Task<ActionResult> DeleteRecipe(int recipeId)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest("You must be logged in.");
                }

                Recipe? r = await _recipeService.GetRecipeById(recipeId);
                if (r != null)
                {
                    if(r.UserId != currentId)
                    {
                        return Unauthorized("You do not have access to delete this recipe");
                    }

                    if (await _recipeService.CheckRecipeUsed(r))
                        return Conflict("Recipe is being used in other posts or recipe revisions, you can dissociate yourself with the recipe or contact an adminstrator to forcefully remove the recipe with valid reasoning.");
                    int numRows = await _recipeService.DeleteRecipe(r);
                    if (numRows > 0)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest("The recipe was not modified, please check submission.");
                    }
                }
                else
                {
                    return NotFound("This recipe you are deleting does not exist.");
                }
                
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpPut("{recipeId}")]
        public async Task<ActionResult> PutRecipe(int recipeId, RecipeUpdateData recipeData)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId)) 
                {
                    return BadRequest("You must be logged in to post a recipe.");
                }

                Recipe? oldRecipe = await _recipeService.GetRecipeById(recipeId);
                if (oldRecipe == null)
                {
                    return NotFound("Recipe does not exist.");
                }
                if (await _recipeService.CheckRecipeUsed(oldRecipe))
                {
                    Recipe newRecipe = new()
                    {
                        CreatedDatetime = DateTime.Now,
                        UserId = currentId,
                        RecipeSteps = new List<RecipeStep>()
                    };

                    if (!recipeData.RecipeTitle.IsNullOrEmpty())
                    {
                        newRecipe.RecipeTitle = recipeData.RecipeTitle;
                    }
                    else
                    {
                        newRecipe.RecipeTitle = oldRecipe.RecipeTitle;
                    }

                    if (!recipeData.RecipeDescription.IsNullOrEmpty())
                    {
                        newRecipe.RecipeDescription = recipeData.RecipeDescription;
                    }
                    else
                    {
                        newRecipe.RecipeDescription = oldRecipe.RecipeDescription;
                    }

                    if (recipeData.RecipeSteps.Count > 0)
                    {
                        for (int si = 0; si < recipeData.RecipeSteps.Count; si++)
                        {
                            RecipeStepData stepData = recipeData.RecipeSteps.ElementAt(si);
                            RecipeStep step = new()
                            {
                                StepDescription = stepData.StepDescription,
                                StepOrder = stepData.StepOrder,
                                StepIngredients = new List<StepIngredient>()
                            };
                            for (int ii = 0; ii < stepData.StepIngredients.Count; ii++)
                            {
                                StepIngredientData ingData = stepData.StepIngredients.ElementAt(ii);

                                step.StepIngredients.Add(new()
                                {
                                    IngredientId = ingData.IngredientId,
                                    IngredientMeasurementUnit = ingData.IngredientMeasurementUnit,
                                    IngredientMeasurementValue = ingData.IngredientMeasurementValue
                                });
                            }
                            newRecipe.RecipeSteps.Add(step);
                        }
                    }
                    else
                    {
                        for (int si = 0; si < oldRecipe.RecipeSteps.Count; si++)
                        {
                            RecipeStep oldStep = oldRecipe.RecipeSteps.ElementAt(si);
                            RecipeStep step = new()
                            {
                                StepDescription = oldStep.StepDescription,
                                StepOrder = oldStep.StepOrder,
                                StepIngredients = new List<StepIngredient>()
                            };
                            for (int ii = 0; ii < oldStep.StepIngredients.Count; ii++)
                            {
                                StepIngredient oldStepIngredient = oldStep.StepIngredients.ElementAt(ii);

                                step.StepIngredients.Add(new()
                                {
                                    IngredientId = oldStepIngredient.IngredientId,
                                    IngredientMeasurementUnit = oldStepIngredient.IngredientMeasurementUnit,
                                    IngredientMeasurementValue = oldStepIngredient.IngredientMeasurementValue
                                });
                            }
                            newRecipe.RecipeSteps.Add(step);
                        }
                    }

                    //Consider adding updated field to our data models. For now i will treat created fields as this.


                    if (await _recipeService.CreateRecipe(newRecipe))
                    {
                        if (await _recipeService.CreateRecipeRevision(oldRecipe.RecipeId, newRecipe.RecipeId))
                        {
                            if (await _recipeService.AddRecipeToCookbook(currentId, newRecipe))
                            {

                                return Ok(new { RevisedRecipeId = newRecipe.RecipeId});
                            }
                            else
                            {
                                return StatusCode(500, new { Message = "New Recipe Created but failed to add to your cookbook", RevisedRecipeId = newRecipe.RecipeId });
                            }
                        }
                        if (await _recipeService.AddRecipeToCookbook(currentId, newRecipe))
                        {

                            return StatusCode(500, new { Message = "New Recipe Created but failed to link to old recipe", RevisedRecipeId = newRecipe.RecipeId });
                        }
                        else
                        {
                            return StatusCode(500, new { Message = "New Recipe Created but failed to add to your cookbook and link to old recipe", RevisedRecipeId = newRecipe.RecipeId });
                        }
                    }
                    return StatusCode(500, "There was an error creating revision of recipe");
                }
                else
                {
                    if (!recipeData.RecipeTitle.IsNullOrEmpty())
                    {
                        oldRecipe.RecipeTitle = recipeData.RecipeTitle;
                    }

                    if (!recipeData.RecipeDescription.IsNullOrEmpty())
                    {
                        oldRecipe.RecipeDescription = recipeData.RecipeDescription;
                    }

                    if (recipeData.RecipeSteps.Count > 0)
                    {
                        await _recipeService.DeleteRecipeSteps(oldRecipe.RecipeSteps);
                        oldRecipe.RecipeSteps.Clear();
                        for (int si = 0; si < recipeData.RecipeSteps.Count; si++)
                        {
                            RecipeStepData stepData = recipeData.RecipeSteps.ElementAt(si);
                            RecipeStep step = new()
                            {
                                StepDescription = stepData.StepDescription,
                                StepOrder = stepData.StepOrder,
                                StepIngredients = new List<StepIngredient>()
                            };
                            for (int ii = 0; ii < stepData.StepIngredients.Count; ii++)
                            {
                                StepIngredientData ingData = stepData.StepIngredients.ElementAt(ii);

                                step.StepIngredients.Add(new()
                                {
                                    IngredientId = ingData.IngredientId,
                                    IngredientMeasurementUnit = ingData.IngredientMeasurementUnit,
                                    IngredientMeasurementValue = ingData.IngredientMeasurementValue
                                });
                            }
                            oldRecipe.RecipeSteps.Add(step);
                        }
                    }

                    var res = await _recipeService.UpdateRecipe(oldRecipe);
                    if(res > 0)
                    {
                        return Ok(new { UpdatedRecipeId = oldRecipe.RecipeId });
                    }
                    return StatusCode(500, "There was an issue updating the recipe");
                }
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpGet("cookbook")]
        public async Task<ActionResult> GetCookbook(string? sortBy)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest("You must be logged in to view your cookbook");
                }

                List<Recipe> recipes = new();
                recipes = await _recipeService.GetRecipeCookbook(currentId, sortBy);

                return Ok(recipes.Select(r => new
                {
                    r.RecipeId,
                    r.RecipeTitle,
                    r.RecipeDescription,
                    r.UserId,
                    r.CreatedDatetime

                }));
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    Debug.Write(ex.Message);
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [AllowAnonymous]
        [HttpGet("cookbook/User/{userId}")]
        public async Task<ActionResult> GetCookbook(int userId, string? sortBy)
        {
            try
            {
                List<Recipe> recipes = new();
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    recipes = await _recipeService.GetRecipeCookbook(userId, sortBy);
                }
                else
                {
                    var blockStatus = await _userService.CheckBlock(currentId, userId);
                    if ((int)blockStatus > 0)
                    {
                        if ((int)blockStatus == 2)
                        {
                            return Unauthorized("User has been blocked");
                        }
                        else
                        {
                            return NotFound();
                        }
                    }

                    recipes = await _recipeService.GetRecipeCookbook(userId, sortBy);
                }
                
                

                return Ok(recipes.Select(r => new
                {
                    r.RecipeId,
                    r.RecipeTitle,
                    r.RecipeDescription,
                    r.UserId,
                    r.CreatedDatetime

                }));
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    Debug.Write(ex.Message);
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpPut("{recipeId}/cookbook")]
        public async Task<ActionResult> AddRecipeToCookbook(int recipeId)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest("You must be logged in");
                }
                if(await _recipeService.CheckCookbookConflict(currentId, recipeId))
                {
                    return Conflict("Recipe is already in your cookbook");
                }

                Recipe? r = await _recipeService.GetRecipeById(recipeId);
                if(r == null)
                {
                    return NotFound("Recipe not found");
                }

                if(await _recipeService.AddRecipeToCookbook(currentId, r ))
                {
                    return Ok();
                }
                return StatusCode(500, "Error Adding Recipe to Cookbook");
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }
        [HttpDelete("{recipeId}/cookbook")]
        public async Task<ActionResult> DeleteRecipeFromCookbook(int recipeId)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest("You must be logged in");
                }

                Recipe? r = await _recipeService.GetRecipeById(recipeId);

                if(r.UserId == currentId)
                {
                    return BadRequest("You cannot remove your own recipes from your cookbook, please delete or dissociate instead");
                }

                if(await _recipeService.RemoveRecipeFromCookbook(currentId, recipeId))
                {
                    return Ok();
                }
                return StatusCode(500, "Error Adding Recipe to Cookbook");
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        //No auth here due to the potential of this being called for non-user viewing of posts.
        [AllowAnonymous]
        [HttpGet("{recipeId}")]
        public async Task<ActionResult> GetRecipeById(int recipeId)
        {
            try
            {
                Recipe? r = await _recipeService.GetRecipeById(recipeId);

                if (r != null)
                {
                    var result = new
                    {
                        RecipeTitle = r.RecipeTitle,
                        RecipeDescription = r.RecipeDescription,
                        CreatedByUsername = r.User?.Username,
                        CreatedDatetime = r.CreatedDatetime,
                        RecipeSteps = r.RecipeSteps.Select(rs => new
                        {
                            rs.StepId,
                            rs.StepDescription,
                            rs.StepOrder,
                            StepIngredients = rs.StepIngredients.Select(si => new
                            {
                                si.StepIngredientId,
                                si.IngredientMeasurementUnit,
                                si.IngredientMeasurementValue,
                                Ingredient = new
                                {
                                    si.Ingredient.IngredientTitle,
                                    si.Ingredient.IngredientDescription,
                                    si.Ingredient.IngredientIcon
                                }
                            })
                        })
                    };
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [AllowAnonymous]
        [HttpGet("{recipeId}/steps")]
        public async Task<ActionResult> GetRecipeSteps(int recipeId)
        {
            try
            {
                List<RecipeStep> steps = await _recipeService.GetRecipeStepsByRecipeId(recipeId);

                if (steps != null)
                {
                    return Ok(steps.Select(rs => new
                    {
                        rs.StepId,
                        rs.StepDescription,
                        rs.StepOrder,
                        StepIngredients = rs.StepIngredients.Select(si => new
                        {
                            si.StepIngredientId,
                            si.IngredientMeasurementUnit,
                            si.IngredientMeasurementValue,
                            Ingredient = new
                            {
                                si.Ingredient.IngredientTitle,
                                si.Ingredient.IngredientDescription,
                                si.Ingredient.IngredientIcon
                            }
                        })
                    }));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        


        [AllowAnonymous]
        [HttpGet("{recipeId}/steps/{stepId}")]
        public async Task<ActionResult> GetRecipeStepById(int stepId)
        {
            try
            {
                RecipeStep? rs = await _recipeService.GetRecipeStepById(stepId);

                if (rs != null)
                {
                    return Ok(new
                    {
                        rs.StepId,
                        rs.StepDescription,
                        rs.StepOrder,
                        StepIngredients = rs.StepIngredients.Select(si => new
                        {
                            si.StepIngredientId,
                            si.IngredientMeasurementUnit,
                            si.IngredientMeasurementValue,
                            Ingredient = new
                            {
                                si.Ingredient.IngredientTitle,
                                si.Ingredient.IngredientDescription,
                                si.Ingredient.IngredientIcon
                            }
                        })
                    });
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        // RECIPE STEP CHANGES
        /*
        [HttpPost("{recipeId}/steps")]
        public async Task<ActionResult> PostRecipeStep(RecipeStepData stepData)
        {
            try
            {
                RecipeStep rs = new()
                {
                    RecipeId = stepData.RecipeId,
                    StepOrder = stepData.StepOrder,
                    StepDescription = stepData.StepDescription
                };
                if (stepData.StepIngredients.Count > 0)
                {
                    foreach(Ingredient i in stepData.StepIngredients)
                    {
                        StepIngredient si = new()
                        {
                            IngredientMeasurementValue = stepData.ingredientMeasurementValue,
                            IngredientMeasurementUnit = stepData.ingredientMeasurementLabel,
                            IngredientId = i.IngredientId
                        };
                        await _recipeService.CreateRecipeStepIngredient(si);
                    }
                }
                if (await _recipeService.CreateRecipeStep(rs) > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("The step was not added, please check submission.");
                }
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }
        
        [HttpPut("{recipeId}/steps/{stepId}")]
        public async Task<ActionResult> PutRecipeStep(int stepId, RecipeStepData recipeStepData)
        {
            try
            {
                RecipeStep? step = await _recipeService.GetRecipeStepById(stepId);
                if (step != null) 
                {
                    foreach (Ingredient i in recipeStepData.StepIngredients)
                    {
                        StepIngredient si = new()
                        {
                            IngredientMeasurementValue = recipeStepData.ingredientMeasurementValue,
                            IngredientMeasurementUnit = recipeStepData.ingredientMeasurementLabel,
                            IngredientId = i.IngredientId
                        };
                        await _recipeService.PutRecipeStepIngredient(si);
                    }
                    step.StepOrder = recipeStepData.StepOrder;
                    step.StepDescription = recipeStepData.StepDescription;

                    int numrows = await _recipeService.UpdateRecipeStep(step);
                    if(numrows > 0)
                    {
                        return Ok(numrows);
                    }
                    else
                    {
                        return BadRequest("Please double check form data and resubmit.");
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }


        [HttpDelete("{recipeId}/steps/{stepId}")]
        public async Task<ActionResult> DeleteRecipeStep(int stepId)
        {
            try
            {
                RecipeStep? rs = await _recipeService.GetRecipeStepById(stepId);
                if (rs != null)
                {
                    if (await _recipeService.DeleteRecipeStep(rs) > 0)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest("The step was not modified, please check submission.");
                    }
                }
                return NotFound("This step you are deleting does not exist.");
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }
        */
    }
}
