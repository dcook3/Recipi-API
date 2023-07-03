using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Services;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipesController(IRecipeService service)
        {
            _recipeService = service;
        }
        //CreateRecipe
        [HttpPost("new")]
        //DeleteRecipe
        [HttpDelete("remove")]
        //GetRecipes
        [HttpGet]
        //GetRecipeById
        [HttpGet("{recipeId}")]
    }
}
