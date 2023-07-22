using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;
using System.Security.Claims;

namespace Recipi_API.Services
{
    public class PostFetchService : IPostFetchService
    {
        private readonly RecipiDbContext context = new();
        private readonly UserService userService = new();

        public PostFetchService(UserService _userService)
        {
            userService = _userService;
        }

        public async Task<List<PostPreview>> GetFollowingPosts(int userId)
        {
            List<User> followedUsers = await userService.GetFollowing(userId);
            List<PostPreview> postPreviews = new();
            List<int> followedUserIds = followedUsers.Select(u => u.UserId).ToList();

            List<Post> posts = await context.Posts.Where(p => followedUserIds.Contains(p.UserId)).ToListAsync();

            foreach (Post p in posts)
            {
                PostPreview postPreview = new PostPreview();
                postPreview.thumbnailURL = p.ThumbnailUrl;
                postPreview.postId = p.PostId;
                postPreviews.Add(postPreview);
            }

            return postPreviews;
        }

        public async Task<List<PostPreview>> GetRecommendedPosts(int offset)
        {
            List<PostPreview> postPreviews = new();
            List<Post> posts = await context.Posts.OrderByDescending(p => p.PostLikes.Count()).Take(1000).ToListAsync();
            foreach (Post p in posts)
            {
                PostPreview postPreview = new();
                postPreview.thumbnailURL = p.ThumbnailUrl;
                postPreview.postId = p.PostId;
                postPreviews.Add(postPreview);
            }
            return postPreviews;
        }

        public async Task<Post> GetSinglePost(int postId) => await context.Posts.FindAsync(postId);

        public async Task<List<PostPreview>> GetUserPosts(int userId)
        {
            List<Post> posts = await context.Posts.Where(p => p.UserId == userId).ToListAsync();
            List<PostPreview> postPreviews = new();
            foreach (Post p in posts)
            {
                PostPreview postPreview = new PostPreview();
                postPreview.thumbnailURL = p.ThumbnailUrl;
                postPreview.postId = p.PostId;
                postPreviews.Add(postPreview);
            }
            return postPreviews;
        }
    }
}
