using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Recipi_API.Models;
using System.Linq;


namespace Recipi_API.Services
{
    public class PostInteractionsService : IPostInteractionsService
    {
        private static RecipiDbContext context = new RecipiDbContext();
        public async Task<List<PostComment>> GetComments(int postId)
        {
            return await context.PostComments.Where(pc => pc.PostId == postId).ToListAsync();
        }

        public async Task<int> PostComment(int postId, int userId, string comment)
        {
            PostComment pc = new PostComment();
            pc.Comment = comment;
            pc.CommentDatetime = DateTime.Now;
            pc.PostId = postId;
            pc.UserId = userId;
            context.PostComments.Add(pc);
            return await context.SaveChangesAsync();
        }

        public async Task<int> PostLike(int postId, int userId)
        {
            PostLike pl = new PostLike();
            pl.PostId = postId;
            pl.UserId = userId;
            context.PostLikes.Add(pl);
            return await context.SaveChangesAsync();
        }

        public async Task<int> PostReport(int postId, int userId, string message)
        {
            PostReport r = new PostReport();
            r.PostId = postId;
            r.UserId = userId;
            r.Message = message;
            r.ReportedDatetime = DateTime.Now;
            context.PostReports.Add(r);
            return await context.SaveChangesAsync();
        }
    }
}
