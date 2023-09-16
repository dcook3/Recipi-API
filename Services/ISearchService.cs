using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface ISearchService
    {
        public Task<List<Recipe>> SearchRecipes(string query);

        public Task<List<User>> SearchUsers(string query);
        
        public Task<List<Post>> SearchPosts(string query);
    }
}
