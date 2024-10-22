﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Recipi_API.Models;
using System.Linq;


namespace Recipi_API.Services
{
    public class PostInteractionsService : IPostInteractionsService
    {
        private readonly static RecipiDbContext context = new();
        public async Task<List<PostComment>> GetComments(int postId)
        {
            return await context.PostComments.Where(pc => pc.PostId == postId).Include(pc => pc.User).ToListAsync();
        }

        public async Task<int> PostComment(int postId, int userId, string comment)
        {
            PostComment pc = new()
            {
                Comment = comment,
                CommentDatetime = DateTime.Now,
                PostId = postId,
                UserId = userId
            };
            context.PostComments.Add(pc);
            return await context.SaveChangesAsync();
        }

        public async Task<PostInteraction?> CreatePostInteraction(int postId, int userId)
        {
            PostInteraction pi = new()
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
            PostReport r = new()
            {
                PostId = postId,
                UserId = userId,
                Message = message,
                ReportedDatetime = DateTime.Now
            };
            context.PostReports.Add(r);
            return await context.SaveChangesAsync();
        }

        public async Task<int> GetLikeCount(int postId)
        {
            return await context.PostInteractions.Where(pi => pi.PostId == postId && pi.Liked == true).CountAsync();
        }

        public async Task<int> GetCommentCount(int postId)
        {
            return await context.PostComments.Where(c => c.PostId == postId).CountAsync();
        }

        public async Task<bool> HasLiked(int postId, int userId)
        {
            return await context.PostInteractions.Where(pi => pi.PostId == postId && pi.Liked == true && pi.UserId == userId).CountAsync() > 0;
        }
    }
}
