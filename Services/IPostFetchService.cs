using Microsoft.Identity.Client;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Services
{
    public interface IPostFetchService
    {
        public Task<List<PostPreview>> GetRecommendedPosts();
        public Task<List<PostPreview>> GetFollowingPosts(int followingId);
        public Task<List<PostPreview>> GetUserPosts(int userId);
        public Task<Post> GetSinglePost(int postId);
    }
}
