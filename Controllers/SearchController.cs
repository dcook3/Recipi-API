using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Services;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {

        private readonly ISearchService _searchService;
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }


        
        [AllowAnonymous]
        [HttpGet("{query}")]
        public async Task<IActionResult> Search(string query)
        {
            if (query == null)
            {
                return BadRequest();
            }
            query = $"%{query}%";

            var recipes = await _searchService.SearchRecipes(query);
            var users = await _searchService.SearchUsers(query);
            var posts = await _searchService.SearchPosts(query);

            return Ok(new
            {
                users = users.Select(user => new
                {
                    user.UserId,
                    user.Username,
                    user.ProfilePicture
                }),
                posts = posts.Select(post => new
                {
                    post.PostId,
                    post.PostTitle
                }),
                recipes = recipes.Select(recipe => new
                {
                    recipe.RecipeId,
                    recipe.RecipeTitle
                })
            });
        }
    }
}
