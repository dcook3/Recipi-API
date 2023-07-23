using Recipi_API.Models;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Services
{
    public class PostService
    {

        private readonly static RecipiDbContext db = new();

        public async Task<bool> CreatePost(PostData postData)
        {
            Post post = new Post();
            db.Posts.Add(post);
            var res = await db.SaveChangesAsync();
            return res == 1;
        }

        public async Task<bool>UpdatePost(int userId, PostData post)
        {
            return false;
        }

    }
}
