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

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly ClaimsIdentity? _claims;

        public RecipesController(IRecipeService recipeService, IHttpContextAccessor _context)
        {
            _recipeService = recipeService;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpPost]
        public async Task<ActionResult> PostRecipe(RecipeData recipe)
        {
            try
            {
                Recipe r = new()
                {
                    RecipeDescription = recipe.RecipeDescription,
                    RecipeTitle = recipe.RecipeTitle,
                    CreatedDatetime = DateTime.Now
                };
                if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    r.UserId = currentId;
                }
                else
                {
                    return BadRequest("You must be logged in to post a recipe.");
                }


                r.RecipeSteps = recipe.RecipeSteps;

                int numRows = await _recipeService.CreateRecipe(r);
                if (numRows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Please enter valid form data.");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpDelete("{recipeId}")]
        public async Task<ActionResult> DeleteRecipe(int recipeId)
        {
            try
            {
                Recipe? r = await _recipeService.GetRecipeById(recipeId);
                if (r != null)
                {
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpPut("{recipeId}")]
        public async Task<ActionResult> PutRecipe(RecipeData recipeData)
        {
            try
            {
                Recipe? r = await _recipeService.GetRecipeById(recipeData.RecipeId);
                if (r != null)
                {
                    r.RecipeId = recipeData.RecipeId;
                    r.RecipeTitle = recipeData.RecipeTitle;
                    r.RecipeDescription = recipeData.RecipeDescription;
                    //Consider adding updated field to our data models. For now i will treat created fields as this.
                    r.CreatedDatetime = DateTime.Now;
                    r.RecipeSteps = recipeData.RecipeSteps;

                    
                    if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                    {
                        r.UserId = currentId;
                    }
                    else
                    {
                        return BadRequest("You must be logged in to post a recipe.");
                    }

                    int numRows = await _recipeService.UpdateRecipe(r);
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetUserRecipes(int userId, string? sortBy)
        {
            try
            {
                List<Recipe> recipes = new();
                if (sortBy != null)
                {
                     recipes = await _recipeService.GetRecipeCookbook(userId, sortBy);
                }
                else
                {
                     recipes = await _recipeService.GetRecipeCookbook(userId);
                }

                if (recipes.Count > 0)
                {
                    return Ok(recipes);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        //No auth here due to the potential of this being called for non-user viewing of posts.
        [HttpGet("{recipeId}")]
        public async Task<ActionResult> GetRecipeById(int recipeId)
        {
            try
            {
                Recipe? r = await _recipeService.GetRecipeById(recipeId);

                if (r != null)
                {
                    return Ok(r);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }


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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }
    }
}
