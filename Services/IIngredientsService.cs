using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IIngredientsService
    {
        public Task<List<StepIngredient>> GetIngredientsForRecipe(int recipeId);
        public Task<int> CreateIngredient(Ingredient ingredient);
        public Task<Ingredient?> GetIngredientById(int ingId);
        public Task<bool> CheckIngredient(int ingId);
        public Task<List<Ingredient>> GetIngredients();
        public Task<List<Ingredient>> SearchIngredients(string keyword);
    }
}
