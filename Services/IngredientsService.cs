using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;

namespace Recipi_API.Services
{
    public class IngredientsService : IIngredientsService
    {
        private readonly RecipiDbContext context = new();
        public async Task<int> CreateIngredient(Ingredient ingredient)
        {
            context.Ingredients.Add(ingredient);
            return await context.SaveChangesAsync();

        }

        public async Task<List<StepIngredient>> GetIngredientsForRecipe(int recipeId)
        {
            return await context.StepIngredients.Where(si => si.Step.RecipeId == recipeId).ToListAsync();
        }

        public async Task<Ingredient?> GetIngredientById(int ingId)
        {
            return await context.Ingredients.FindAsync(ingId);
        }
        public async Task<bool> CheckIngredient(int ingId)
        {
            return await context.Ingredients.AnyAsync(i => i.IngredientId == ingId);
        }

        public async Task<List<Ingredient>> GetIngredients() => await context.Ingredients.ToListAsync();
    }
}
