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
        private readonly ClaimsIdentity? _claims;

        public RecipesController(IRecipeService recipeService, IIngredientsService ingService, IHttpContextAccessor _context)
        {
            _recipeService = recipeService;
            _ingService = ingService;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
        }

        [HttpPost]
        public async Task<ActionResult> PostRecipe(RecipeData recipe)
        {
            //try
            //{
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

                if (!await _recipeService.CreateRecipe(r))
                {
                    return StatusCode(500, "Error creating recipe");
                }

                List<RecipeStep> steps = new();
                List<StepIngredient> stepIngredients = new();
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

                    if (!await _recipeService.AddRecipeStep(step))
                    {
                        return StatusCode(500, "Error adding recipe steps");
                    }

                    for (int ii = 0; ii < stepData.StepIngredients.Count; ii++)
                    {
                        StepIngredientData ingData = stepData.StepIngredients.ElementAt(ii);

                        StepIngredient stepIngredient = new()
                        {
                            IngredientId = ingData.IngredientId,
                            IngredientMeasurementUnit = ingData.IngredientMeasurementUnit,
                            IngredientMeasurementValue = ingData.IngredientMeasurementValue,
                            StepId = step.StepId
                        };
                        if (!await _recipeService.AddStepIngredient(stepIngredient))
                        {
                            return StatusCode(500, "Error adding step ingredients");
                        }
                    }
                    
                }

                
                if(!await _recipeService.AddRecipeToCookbook(currentId, r))
                {
                    return StatusCode(500, "Recipe Created but failed to add to your cookbook");
                }
                return Ok(r);
                //Recipe r = await _recipeService.CreateRecipe(currentId, recipe);
                if (r.RecipeId > 0)
                {
                    if (await _recipeService.AddRecipeToCookbook(currentId, r))
                    {

                        return Ok(r);
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
                try { 
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

                    if (await _recipeService.CheckRecipeUsed(recipeId))
                        return Conflict("Recipe is being used in other posts, you can dissociate yourself with the recipe or contact an adminstrator to forcefully remove the recipe with valid reasoning.");
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
        public async Task<ActionResult> PutRecipe(int recipeId, RecipeData recipeData)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId)) 
                {
                    return BadRequest("You must be logged in to post a recipe.");
                }

                Recipe? oldRecipe = await _recipeService.GetRecipeById(recipeId);
                if (oldRecipe != null)
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
                            if (!await _ingService.CheckIngredient(ingData.IngredientId))
                            {
                                return BadRequest("Ingredient does not exist");
                            }
                            step.StepIngredients.Add(new()
                            {
                                IngredientId = ingData.IngredientId,
                                IngredientMeasurementUnit = ingData.IngredientMeasurementUnit,
                                IngredientMeasurementValue = ingData.IngredientMeasurementValue
                            });
                        }
                        newRecipe.RecipeSteps.Add(step);
                    }

                    //Consider adding updated field to our data models. For now i will treat created fields as this.





                    int numRows = 0;// await _recipeService.CreateRecipe(newRecipe);
                    numRows += await _recipeService.CreateRecipeRevision(oldRecipe.RecipeId, newRecipe.RecipeId);
                    if (numRows > 1)
                    {
                        if (await _recipeService.AddRecipeToCookbook(currentId, newRecipe))
                        {

                            return Ok(newRecipe);
                        }
                        else
                        {
                            return StatusCode(500, "New Recipe Created but faild to add to your cookbook");
                        }
                    }

                    return StatusCode(500, "There was an error creating revision of recipe");
                }
                else
                {
                    return NotFound("Recipe does not exist.");
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
                if (sortBy != null)
                {
                     recipes = await _recipeService.GetRecipeCookbook(currentId, sortBy);
                }
                else
                {
                     recipes = await _recipeService.GetRecipeCookbook(currentId);
                }

                if (recipes.Count > 0)
                {
                    return Ok(recipes);
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
                List<RecipeStep> r = await _recipeService.GetRecipeStepsByRecipeId(recipeId);

                if (r != null)
                {
                    return Ok(r);
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
                RecipeStep? step = await _recipeService.GetRecipeStepById(stepId);

                if (step != null)
                {
                    return Ok(step);
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
