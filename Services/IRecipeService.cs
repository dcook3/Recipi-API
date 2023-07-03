using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IRecipeService
    {
        public Task<List<Recipe>> GetRecipeCookbook(int userId, string sortBy);
        public Task<List<Recipe>> GetRecipeCookbook(int userId);
        public Task<Recipe> GetRecipeById(int recipeId);
        public Task<int> CreateRecipe(Recipe recipe);
        public Task<int> DeleteRecipe(int recipeId);
    }
}
