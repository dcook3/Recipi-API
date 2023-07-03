using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;

namespace Recipi_API.Services
{
    public class RecipeService : IRecipeService
    {
        private static RecipiDbContext context = new RecipiDbContext();

        public async Task<int> CreateRecipe(Recipe recipe)
        {
            context.Recipes.Add(recipe);
            return await context.SaveChangesAsync();
        }

        
        public async Task<int> UpdateRecipe(Recipe recipe)
        {
            context.Recipes.Update(recipe);
            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteRecipe(Recipe recipe)
        {
            context.Recipes.Remove(recipe);
            return await context.SaveChangesAsync();
        }

        public async Task<RecipeStep>? GetRecipeStepById(int stepId)
        {
            return await context.RecipeSteps.FindAsync(stepId);
        }

        public async Task<List<RecipeStep>> GetRecipeStepsByRecipeId(int recipeId)
        {
            return await context.RecipeSteps.Where(rs => rs.RecipeId == recipeId).ToListAsync();
        }

        public async Task<int> CreateRecipeStep(RecipeStep recipeStep)
        {
            context.RecipeSteps.Add(recipeStep);
            return await context.SaveChangesAsync();
        }

        public async Task<int> UpdateRecipeStep(RecipeStep recipeStep)
        {
            context.RecipeSteps.Update(recipeStep);
            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteRecipeStep(RecipeStep recipeStep)
        {
            context.RecipeSteps.Remove(recipeStep);
            return await context.SaveChangesAsync();
        }

        public async Task<Recipe>? GetRecipeById(int recipeId)
        {
            return await context.Recipes.FindAsync(recipeId);
        }

        public async Task<List<Recipe>> GetRecipeCookbook(int userId, string sortBy)
        {
            //Could add more, depending on which sort keys we wish to use.
            if(sortBy == "author")
            {
                return await context.Recipes.Where(r => r.UserId == userId).OrderBy(r => r.User.Username).ToListAsync();
            }
            else
            {
                return await context.Recipes.Where(r => r.UserId == userId).OrderBy(r => r.CreatedDatetime).ToListAsync();
            }
            
        }

        public async Task<List<Recipe>> GetRecipeCookbook(int userId)
        {
            return await context.Recipes.Where(r => r.UserId == userId).ToListAsync();
        }

        public async Task<int> CreateRecipeStepIngredient(StepIngredient stepIngredient)
        {
            context.StepIngredients.Add(stepIngredient); 
            return await context.SaveChangesAsync();
        }
        public async Task<int> PutRecipeStepIngredient(StepIngredient stepIngredient)
        {
            context.StepIngredients.Update(stepIngredient);
            return await context.SaveChangesAsync();
        }
    }
}
