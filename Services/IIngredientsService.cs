using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IIngredientsService
    {
        public Task<List<Ingredient>> GetIngredientsForRecipe(RecipeData recipe);
        public Task<int> CreateIngredient(Ingredient ing);
    }
}
