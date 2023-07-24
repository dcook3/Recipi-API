using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IPostService
    {
        Task<bool> CreatePost(Post post);
        Task<bool> DeletePost(Post post);
        Task<bool> UpdatePost(Post post);
        Task<bool> DeletePostMedia(int postId);
    }
}