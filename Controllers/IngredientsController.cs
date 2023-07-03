using Microsoft.AspNetCore.Mvc;
using Recipi_API.Services;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientsService _ingredientsService;

        public IngredientsController(IIngredientsService service)
        {
            _ingredientsService = service;
        }
        //GetIngredientsForRecipe
        [HttpGet("{recipeId}")]
        //CreateIngredient
        [HttpPost("new")]
    }
}
