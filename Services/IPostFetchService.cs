using Microsoft.Identity.Client;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;
using System.Transactions;

namespace Recipi_API.Services
{
    public interface IPostFetchService
    {
        public Task<List<PostPreview>> GetRecommendedPosts(int num);
        public Task<List<PostPreview>> GetFollowingPosts(int userId);
        public Task<List<PostPreview>> GetUserPosts(int userId);
        public Task<Post> GetSinglePost(int postId);
    }
}
