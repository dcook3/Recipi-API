﻿using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;
using System.Security.Claims;

namespace Recipi_API.Services
{
    public class PostFetchService : IPostFetchService
    {
        private readonly RecipiDbContext context = new();
        private readonly IUserService userService;

        public PostFetchService(IUserService _userService)
        {
            userService = _userService;
        }

        public async Task<List<PostPreview>> GetFollowingPosts(int userId)
        {
            List<User> followedUsers = await userService.GetFollowing(userId);
            List<PostPreview> postPreviews = new();
            List<int> followedUserIds = followedUsers.Select(u => u.UserId).ToList();

            List<Post> posts = await context.Posts.Where(p => followedUserIds.Contains(p.UserId)).ToListAsync();

            foreach (Post p in posts)
            {
                PostPreview postPreview = new()
                {
                    thumbnailURL = p.ThumbnailUrl,
                    postId = p.PostId
                };
                postPreviews.Add(postPreview);
            }

            return postPreviews;
        }
        public async Task<List<PostPreview>> GetRecommendedPosts()
        {
            List<PostPreview> postPreviews = new();

            //https://stackoverflow.com/questions/7927329/sql-ordering-records-by-weight could do something like this with dates today bucket, this week bucket, this month bucket
            List<Post> posts = await context.Posts.OrderByDescending(p => p.PostInteractions.Where(pi => pi.Liked == true).Count())
                                                  .ToListAsync();
            /*
                                                  .OrderByDescending(p => p.PostInteractions.GroupBy(pi => pi.PostId)
                                                                                  .Select(pi => pi.Select(pi => pi.Liked == true).Count()))*/
            foreach (Post p in posts)
            {
                PostPreview postPreview = new()
                {
                    thumbnailURL = p.ThumbnailUrl,
                    postId = p.PostId
                };
                postPreviews.Add(postPreview);
            }
            return postPreviews;
        }
        public async Task<List<PostPreview>> GetRecommendedPosts(int userId)
        {
            List<PostPreview> postPreviews = new();

            //https://stackoverflow.com/questions/7927329/sql-ordering-records-by-weight could do something like this with dates today bucket, this week bucket, this month bucket
            List<Post> posts = await context.Posts.OrderBy(p => p.PostInteractions.Where(pi => pi.UserId == userId).Count())
                                                  .ThenByDescending(p => p.PostInteractions.Where(pi => pi.Liked == true).Count())
                                                  .ToListAsync();
            /*
                                                  .OrderByDescending(p => p.PostInteractions.GroupBy(pi => pi.PostId)
                                                                                  .Select(pi => pi.Select(pi => pi.Liked == true).Count()))*/
            foreach (Post p in posts)
            {
                PostPreview postPreview = new()
                {
                    thumbnailURL = p.ThumbnailUrl,
                    postId = p.PostId
                };
                postPreviews.Add(postPreview);
            }
            return postPreviews;
        }

        public async Task<Post?> GetSinglePost(int postId) => await context.Posts.Where(p => p.PostId == postId)
                                                                                 .Include(p => p.User)
                                                                                 .Include(p => p.PostMediaNavigation)                                       
                                                                                 .Include(p => p.Recipe)
                                                                                    .ThenInclude(r => r!.RecipeSteps)
                                                                                        //.ThenInclude(rs => rs.PostMedia)
                                                                                 .Include(p => p.Recipe)
                                                                                    .ThenInclude(r => r!.RecipeSteps)
                                                                                        .ThenInclude(rs => rs.StepIngredients)
                                                                                            .ThenInclude(si => si.Ingredient)
                                                                                 .FirstOrDefaultAsync();

        public async Task<List<PostPreview>> GetUserPosts(int userId)
        {
            List<Post> posts = await context.Posts.Where(p => p.UserId == userId).ToListAsync();
                                                    
            List<PostPreview> postPreviews = new();
            foreach (Post p in posts)
            {
                PostPreview postPreview = new()
                {
                    thumbnailURL = p.ThumbnailUrl,
                    postId = p.PostId
                };
                postPreviews.Add(postPreview);
            }
            return postPreviews;
        }
    }
}
