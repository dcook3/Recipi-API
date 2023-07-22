using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;
using Recipi_API.Services;
using System.Data;
using System.Security.Claims;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientsService _ingredientsService;
        private readonly ClaimsIdentity? _claims;

        public IngredientsController(IIngredientsService service, IHttpContextAccessor _context)
        {
            _ingredientsService = service;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
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
        //CreateIngredient
        [HttpPost("")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        public async Task<ActionResult> CreateIngredient(IngredientData ingData)
        {
            try
            {
                Ingredient i = new();
                i.IngredientTitle = ingData.IngredientTitle;
                i.CreatedByUserId = ingData.CreatedByUserId;
                if (ingData.IngredientDescription != null)
                {
                    i.IngredientDescription = ingData.IngredientDescription;
                }
                if (ingData.IngredientIcon != null)
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
    }
}
