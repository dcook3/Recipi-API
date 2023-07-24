using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly RecipiDbContext context = new();

        public async Task<bool> CreateRecipe(Recipe recipe)
        {
            context.Recipes.Add(recipe);
            var res = await context.SaveChangesAsync();
            return res > 0;
        }
        public async Task<bool> AddRecipeStep(RecipeStep step)
        {
            context.RecipeSteps.Add(step);
            var res = await context.SaveChangesAsync();
            return res == 1;
        }
        public async Task<bool> AddStepIngredient(StepIngredient stepIngredient)
        {
            context.StepIngredients.Add(stepIngredient);
            var res = await context.SaveChangesAsync();
            return res == 1;
        }

        public async Task<Recipe> CreateRecipe(int userId, RecipeData recipe)
        {
            Recipe r = new()
            {
                RecipeDescription = recipe.RecipeDescription,
                RecipeTitle = recipe.RecipeTitle,
                CreatedDatetime = DateTime.Now,
                UserId = userId
            };

            context.Recipes.Add(r);
            
            int changeCount = await context.SaveChangesAsync();

            int countCheck = 1;
            for (int si = 0; si < recipe.RecipeSteps.Count; si++)
            {
                
                RecipeStepData stepData = recipe.RecipeSteps.ElementAt(si);
                RecipeStep step = new()
                {
                    StepDescription = stepData.StepDescription,
                    StepOrder = stepData.StepOrder,
                    RecipeId = r.RecipeId
                };
                r.RecipeSteps.Add(step);
                await context.SaveChangesAsync();
                countCheck++;
                for (int ii = 0; ii < stepData.StepIngredients.Count; ii++)
                {
                    StepIngredientData ingData = stepData.StepIngredients.ElementAt(ii);
                    StepIngredient stepIng = new()
                    {
                        IngredientId = ingData.IngredientId,
                        IngredientMeasurementUnit = ingData.IngredientMeasurementUnit,
                        IngredientMeasurementValue = ingData.IngredientMeasurementValue
                    };
                    step.StepIngredients.Add(stepIng);
                    await context.SaveChangesAsync();
                    countCheck++;
                }
            }

            return r;

        }

        public async Task<int> CreateRecipeRevision(int oldRecipeId, int newRecipeId)
        {
            context.RecipeRevisions.Add(new()
            {
                OldRecipeId = oldRecipeId,
                NewRecipeId = newRecipeId,
                Revision = "Recipe Updated"
            });
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

        public async Task<RecipeStep?> GetRecipeStepById(int stepId)
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

        public async Task<Recipe?> GetRecipeById(int recipeId)
        {
            return await context.Recipes.Where(r => r.RecipeId == recipeId)
                                        .Include(r => r.RecipeSteps)
                                        .ThenInclude(rs => rs.StepIngredients)
                                        .ThenInclude(si => si.Ingredient)
                                        .Include(r => r.User)
                                        .FirstOrDefaultAsync();
        }
        public async Task<Recipe?> GetRecipeWithStepsById(int recipeId)
        {
            return await context.Recipes.Where(r => r.RecipeId == recipeId)
                                        .Include(r => r.RecipeSteps)
                                        .FirstOrDefaultAsync();
        }
        public async Task<bool> CheckCookbookConflict(int userId, int recipe_id)
        {
            return await context.RecipeCookbooks.AnyAsync(rc => rc.UserId == userId && rc.RecipeId == recipe_id);
        }
        public async Task<bool> AddRecipeToCookbook(int userId, Recipe recipe)
        {
            int? nextOrder = await context.RecipeCookbooks.Where(rc => rc.UserId == userId)
                                                               .MaxAsync(rc => (int?)rc.RecipeOrder);
            nextOrder ??= 0;
            nextOrder++;
            context.RecipeCookbooks.Add(new()
            {
                UserId = userId,
                Recipe = recipe,
                RecipeOrder = (short)nextOrder
            });
            var res = await context.SaveChangesAsync();
            return res > 0;

        }
        public async Task<bool> RemoveRecipeFromCookbook(int userId, int recipe_id)
        {
            RecipeCookbook? rc = await context.RecipeCookbooks.Where(rc => rc.UserId == userId && rc.RecipeId == recipe_id)
                                                   .FirstOrDefaultAsync();
            if (rc == null) return true;

            context.RecipeCookbooks.Remove(rc);
            var res = await context.SaveChangesAsync();
            return res > 0;

        }
        public async Task<List<Recipe>> GetRecipeCookbook(int userId, string sortBy)
        {
            //Could add more, depending on which sort keys we wish to use.
            if(sortBy == "author")
            {
                return await context.RecipeCookbooks.Where(rc => rc.UserId == userId)
                                                    .Select(rc => rc.Recipe)
                                                    .OrderBy(r => r.User.Username)
                                                    .ThenBy(r => r.CreatedDatetime)
                                                    .ToListAsync();
            }
            else
            {
                return await context.RecipeCookbooks.Where(rc => rc.UserId == userId)
                                                    .Select(rc => rc.Recipe)
                                                    .OrderBy(r => r.CreatedDatetime)
                                                    .ToListAsync();
            }
        }

        public async Task<List<Recipe>> GetRecipeCookbook(int userId)
        {
            return await context.RecipeCookbooks.Where(rc => rc.UserId == userId)
                                        .Select(rc => rc.Recipe)
                                        .ToListAsync();
        }

        public async Task<bool> CheckRecipeUsed(int recipeId)
        {
            return await context.Posts.AnyAsync(p => p.RecipeId == recipeId) 
                || await context.RecipeCookbooks.AnyAsync(rc => rc.RecipeId == recipeId);
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
