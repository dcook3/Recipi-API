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

        public async Task<PostInteraction?> CreatePostInteraction(int postId, int userId)
        {
            PostInteraction pi = new PostInteraction()
            {
                PostId = postId,
                UserId = userId,
                Liked = false,
                InteractionDatetime = DateTime.Now
            };
            context.Add(pi);
            
            if(await context.SaveChangesAsync() == 1)
            {
                return pi;
            }
            else
            {
                return null;
            }
        }
        public async Task<int> PostLike(int postId, int userId)
        {

            var pi = await context.PostInteractions.Where(pi => pi.PostId == postId && pi.UserId == userId).FirstOrDefaultAsync();
            
            if(pi == null)
            {
                if(await context.Posts.AnyAsync(pi => pi.PostId == postId)) 
                { 
                    pi = await this.CreatePostInteraction(postId, userId);
                    if(pi == null)
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }

            pi.Liked = !pi.Liked;
            pi.InteractionDatetime = DateTime.Now;
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
