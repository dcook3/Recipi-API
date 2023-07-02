using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IPostInteractionsService
    {
        public Task<List<PostComment>> GetComments(int postId);
        public Task<int> PostComment(int postId, string comment);
        public Task<int> PostLike(int postId, int userId);
        public Task<int> PostReport(int postId, string message);
    }
}
