using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Services;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly UserService _userService;

        public RecipesController(IRecipeService recipeService, UserService userService)
        {
            _recipeService = recipeService;
            _userService = userService;
        }

        //CreateRecipe
        [HttpPost]
        public async Task<ActionResult> PostRecipe(RecipeData recipe)
        {
            try
            {
                Recipe r = new();
                r.RecipeDescription = recipe.RecipeDescription;
                r.RecipeTitle = recipe.RecipeTitle;
                r.CreatedDatetime = DateTime.Now;

                User? u = await _userService.GetUser(recipe.UserId);
                if (u != null)
                {
                    r.UserId = u.UserId;
                }
                else
                {
                    return NotFound();
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

        //DeleteRecipe
        [HttpDelete("{recipeId}")]
        public async Task<ActionResult> DeleteRecipe(int recipeId)
        {
            try
            {
                Recipe r = await _recipeService.GetRecipeById(recipeId);
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

        [HttpPut("{recipeId}")]
        public async Task<ActionResult> PutRecipe(RecipeData recipeData)
        {
            try
            {
                Recipe r = await _recipeService.GetRecipeById(recipeData.RecipeId);
                if (r != null)
                {
                    r.RecipeId = recipeData.RecipeId;
                    r.RecipeTitle = recipeData.RecipeTitle;
                    r.RecipeDescription = recipeData.RecipeDescription;
                    //Consider adding updated field to our data models. For now i will treat created fields as this.
                    r.CreatedDatetime = DateTime.Now;
                    r.RecipeSteps = recipeData.RecipeSteps;
                    User? u = await _userService.GetUser(recipeData.UserId);
                    if (u != null)
                    {
                        r.User = u;
                    }
                    else
                    {
                        return BadRequest("The recipe was not modified, please check submission.");
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

        //GetRecipes
        [HttpGet]
        public async Task<ActionResult> GetRecipes(int userId, string? sortBy)
        {
            try
            {
                List<Recipe> recipes = new List<Recipe>();
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

        //GetRecipeById
        [HttpGet("{recipeId}")]
        public async Task<ActionResult> GetRecipeById(int recipeId)
        {
            try
            {
                Recipe r = await _recipeService.GetRecipeById(recipeId);

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

        [HttpPost("{recipeId}/steps")]
        public async Task<ActionResult> PostRecipeStep(RecipeStepData step)
        {
            try
            {
                RecipeStep rs = new();
                rs.RecipeId = step.RecipeId;
                rs.StepOrder = step.StepOrder;
                rs.StepDescription = step.StepDescription;
                if(step.StepIngredients.Count > 0)
                {
                    //I dont think this will add to the table... needs testing. Replace with a new db method located in recipes service if needed.
                    rs.StepIngredients = step.StepIngredients;
                }
                rs.PostMedia = step.PostMedia;
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
        public async Task<ActionResult> GetRecipeStepById(int recipeId, int stepId)
        {
            try
            {
                RecipeStep r = await _recipeService.GetRecipeStepById(stepId);

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
        [HttpPut("{recipeId}/steps/{stepId}")]
        public async Task<ActionResult> PutRecipeStep(int stepId, RecipeStepData recipeStepData)
        {
            try
            {
                RecipeStep step = await _recipeService.GetRecipeStepById(stepId);
                if (step != null) 
                {
                    step.StepIngredients = recipeStepData.StepIngredients;
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
        //DeleteRecipe
        [HttpDelete("{recipeId}/steps/{stepId}")]
        public async Task<ActionResult> DeleteRecipeStep(int stepId)
        {
            try
            {
                RecipeStep rs = await _recipeService.GetRecipeStepById(stepId);
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
