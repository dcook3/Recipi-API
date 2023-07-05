using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;
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
        public async Task<ActionResult> GetIngredientsForRecipe(int recipeId)
        {
            try
            {
                List<StepIngredient> sis = await _ingredientsService.GetIngredientsForRecipe(recipeId);
                if(sis.Count > 0)
                {
                    Dictionary<string, Ingredient> ingDict = new Dictionary<string, Ingredient>();
                    sis.ForEach(si =>
                    {
                        Ingredient ing = si.Ingredient;
                        ingDict.Add(ing.IngredientTitle, ing);
                    });
                    return Ok(sis);
                }
                else
                {
                    return BadRequest("Please check form data.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        //CreateIngredient
        [HttpPost("new")]
        public async Task<ActionResult> CreateIngredient(IngredientData ingData)
        {
            Ingredient i = new();
            i.IngredientTitle = ingData.IngredientTitle;
            i.CreatedByUserId = ingData.CreatedByUserId;
            if(ingData.IngredientDescription != null)
            {
                i.IngredientDescription = ingData.IngredientDescription;
            }
            if(ingData.IngredientIcon != null)
            {
                i.IngredientIcon = ingData.IngredientIcon;
            }

            int numRows = await _ingredientsService.CreateIngredient(i);
            if (numRows > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Please check form data.");
            }
        }
    }
}
