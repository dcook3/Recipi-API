using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IRecipeService
    {
        public Task<List<Recipe>> GetRecipeCookbook(int userId, string sortBy);
        public Task<List<Recipe>> GetRecipeCookbook(int userId);
        public Task<Recipe?> GetRecipeById(int recipeId);
        public Task<RecipeStep?> GetRecipeStepById(int stepId);
        public Task<List<RecipeStep>> GetRecipeStepsByRecipeId(int recipeId);
        public Task<int> CreateRecipe(Recipe recipe);
        public Task<int> UpdateRecipe(Recipe recipe);
        public Task<int> DeleteRecipe(Recipe recipe);
        public Task<int> CreateRecipeStep(RecipeStep step);
        public Task<int> UpdateRecipeStep(RecipeStep step);
        public Task<int> DeleteRecipeStep(RecipeStep step);
        public Task<int> CreateRecipeStepIngredient(StepIngredient stepIngredient);
        public Task<int> PutRecipeStepIngredient(StepIngredient stepIngredient);
    }
}
