using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IIngredientsService
    {
        public Task<List<Ingredient>> GetIngredientsForRecipe(Recipe recipe);
        public Task<int> CreateIngredient(Ingredient ing);
    }
}
