using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Services;
using Recipi_API.Models;
using System.Globalization;

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
        [HttpPost("new")]
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
                    r.User = u;
                }
                else
                {
                    return NotFound();
                }

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
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
            }
        }

        //DeleteRecipe
        [HttpDelete("remove")]
        public async Task<ActionResult> DeleteRecipe(int recipeId)
        {
            try
            {
                int numRows = await _recipeService.DeleteRecipe(recipeId);
                if (numRows > 0)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
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
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
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
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
            }
        }
    }
}
