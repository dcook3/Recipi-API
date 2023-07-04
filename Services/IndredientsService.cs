using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;

namespace Recipi_API.Services
{
    public class IndredientsService : IIngredientsService
    {
        private readonly RecipiDbContext _context;
        private readonly UserService _userService;
        public IndredientsService(RecipiDbContext context, UserService userService) 
        { 
            _context = context; 
            _userService = userService;
        }
        public async Task<int> CreateIngredient(Ingredient ingredient)
        {
            _context.Ingredients.Add(ingredient);
            return await _context.SaveChangesAsync();

        }

        public async Task<List<StepIngredient>> GetIngredientsForRecipe(int recipeId)
        {
            return await _context.StepIngredients.Where(si => si.Step.RecipeId == recipeId).ToListAsync();
        }

        public async Task<Ingredient>? GetIngredientById(int ingId)
        {
            return await _context.Ingredients.FindAsync(ingId);
        }
    }
}
