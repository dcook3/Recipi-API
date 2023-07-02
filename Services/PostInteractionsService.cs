using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Recipi_API.Models;
using System.Linq;


namespace Recipi_API.Services
{
    public class PostInteractionsService : IPostInteractionsService
    {
        private readonly IDbContextFactory<RecipiDbContext> _contextFactory;
        public PostInteractionsService(IDbContextFactory<RecipiDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public async Task<List<PostComment>> GetComments(int postId)
        {
            using(var context = _contextFactory.CreateDbContext())
            {
                return await context.PostComments.Where(pc => pc.PostId == postId).ToListAsync();
            }
        }

        public async Task<int> PostComment(int postId, string comment)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                PostComment pc = new PostComment();
                pc.PostId = postId;
                pc.Comment = comment;
                pc.CommentDatetime = DateTime.Now;
                context.PostComments.Add(pc);
                return await context.SaveChangesAsync();
            }
        }

        public async Task<int> PostLike(int postId, int userId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                PostLike pl = new PostLike();
                pl.PostId = postId;
                pl.UserId = userId;
                context.PostLikes.Add(pl);
                return await context.SaveChangesAsync();
            }
        }

        public async Task<int> PostReport(int postId, string message)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                PostReport r = new PostReport();
                r.PostId = postId;
                r.Message = message;
                r.ReportedDatetime = DateTime.Now;
                context.PostReports.Add(r);
                return await context.SaveChangesAsync();
            }
        }
    }
}
