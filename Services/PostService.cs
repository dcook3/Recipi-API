using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Services
{
    public class PostService : IPostService
    {

        private readonly static RecipiDbContext db = new();

        public async Task<bool> CreatePost(Post post)
        {
            db.Posts.Add(post);
            var res = await db.SaveChangesAsync();
            return res > 0;
        }

        public async Task<bool> UpdatePost(Post post)
        {
            db.Posts.Update(post);
            var res = await db.SaveChangesAsync();
            return res > 0;
        }

        public async Task<bool> DeletePost(Post post)
        {
            db.Posts.Remove(post);
            var res = await db.SaveChangesAsync();
            return res > 0;
        }

        public async Task<bool> DeletePostMedia(int postId)
        {
            List<PostMedium> pm = await db.PostMedia.Where(pm => pm.PostId == postId).ToListAsync();
            db.PostMedia.RemoveRange(pm);
            var res = await db.SaveChangesAsync();
            return res == pm.Count;
        }
    }
}
