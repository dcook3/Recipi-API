using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipiDBlib.Models;
using System.Linq;


namespace Recipi_API.Services
{
    public class PostCommentService : IPostCommentService
    {
        private readonly RecipiDbContext _context;
        public PostCommentService(RecipiDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<List<PostComment>>> GetComments(int postId)
        {
            return await _context.PostComments.Where(pc => pc.PostId == postId).ToListAsync();
        }

        public async Task<int> PostComment(int postId, string comment)
        {
            PostComment pc = new PostComment();
            pc.PostId = postId;
            pc.Comment = comment;
            pc.CommentDatetime = DateTime.Now;
            _context.PostComments.Add(pc);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> PostLike(int postId, int userId)
        {
            PostLike pl = new PostLike();
            pl.PostId = postId;
            pl.UserId = userId;
            _context.PostLikes.Add(pl);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> PostReport(int postId, string message)
        {
            PostReport r = new PostReport();
            r.PostId = postId;
            r.Message = message;
            r.ReportedDatetime = DateTime.Now;
            _context.PostReports.Add(r);
            return await _context.SaveChangesAsync();
        }
    }
}
