using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;

namespace Recipi_API.Services
{
    public class RecipeService : IRecipeService
    {
        private static RecipiDbContext context = new RecipiDbContext();

        public async Task<int> CreateRecipe(Recipe recipe)
        {
            context.Add(recipe);
            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteRecipe(int recipeId)
        {
            context.Remove(recipeId);
            return await context.SaveChangesAsync();
        }

        public async Task<Recipe> GetRecipeById(int recipeId)
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
    }
}
