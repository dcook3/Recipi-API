using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IIngredientsService
    {
        public Task<List<StepIngredient>> GetIngredientsForRecipe(int recipeId);
        public Task<int> CreateIngredient(Ingredient ingredient);
        public Task<Ingredient>? GetIngredientById(int ingId);
    }
}
