using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Services
{
    public class PostFetchService : IPostFetchService
    {
        private readonly RecipiDbContext context = new();
        public async Task<List<PostPreview>> GetFollowingPosts(int followingId)
        {
            List<Post> posts = await context.Posts.Where(p => p.UserId == followingId).ToListAsync();
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

        public async Task<List<PostPreview>> GetRecommendedPosts()
        {
            List<PostPreview> postPreviews = new();
            List<Post> posts = new(); //Make this into a list sorted by postlikes on db
            foreach (Post p in posts)
            {
                PostPreview postPreview = new PostPreview();
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
