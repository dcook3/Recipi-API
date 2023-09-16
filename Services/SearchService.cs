using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Recipi_API.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Recipi_API.Services
{

    public class SearchService : ISearchService
    {
        private readonly RecipiDbContext context = new();
        public async Task<List<Post>> SearchPosts(string query) => await context.Posts.Where(post => EF.Functions.Like(post.PostTitle, query)).ToListAsync();

        public async Task<List<Recipe>> SearchRecipes(string query) => await context.Recipes.Where(recipe => EF.Functions.Like(recipe.RecipeTitle, query)).ToListAsync();

        public async Task<List<User>> SearchUsers(string query) => await context.Users.Where(user => EF.Functions.Like(user.Username, query)).ToListAsync();


    }
}
