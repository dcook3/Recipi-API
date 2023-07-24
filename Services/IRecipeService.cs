using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IRecipeService
    {
        public Task<bool> CheckCookbookConflict(int userId, int recipe_id);
        public Task<bool> AddRecipeToCookbook(int userId, Recipe recipe);
        public Task<bool> RemoveRecipeFromCookbook(int userId, int recipe_id);
        public Task<List<Recipe>> GetRecipeCookbook(int userId, string sortBy);
        public Task<List<Recipe>> GetRecipeCookbook(int userId);
        public Task<Recipe?> GetRecipeById(int recipeId);
        public Task<Recipe?> GetRecipeWithStepsById(int recipeId);
        public Task<RecipeStep?> GetRecipeStepById(int stepId);
        public Task<List<RecipeStep>> GetRecipeStepsByRecipeId(int recipeId);
        public Task<bool> CreateRecipe(Recipe recipe);
        public Task<bool> AddRecipeStep(RecipeStep step);
        public Task<bool> AddStepIngredient(StepIngredient stepIngredient);
        public Task<Recipe> CreateRecipe(int userId, RecipeData recipe);
        public Task<int> CreateRecipeRevision(int oldRecipeId, int newRecipeId);
        public Task<int> UpdateRecipe(Recipe recipe);
        public Task<int> DeleteRecipe(Recipe recipe);
        public Task<int> CreateRecipeStep(RecipeStep step);
        public Task<int> UpdateRecipeStep(RecipeStep step);
        public Task<int> DeleteRecipeStep(RecipeStep step);
        public Task<bool> CheckRecipeUsed(int recipeId);
        
        public Task<int> CreateRecipeStepIngredient(StepIngredient stepIngredient);
        public Task<int> PutRecipeStepIngredient(StepIngredient stepIngredient);
    }
}
