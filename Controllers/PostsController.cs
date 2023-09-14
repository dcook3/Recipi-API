using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models.Data_Models;
using Recipi_API.Models;
using Recipi_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostInteractionsService _interactionsService;
        private readonly IPostFetchService _fetchService;
        private readonly ClaimsIdentity? _claims;
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IRecipeService _recipeService;

        public PostsController(IPostService postService, IRecipeService recipeService, IPostInteractionsService interactionService, IPostFetchService fetchService, IHttpContextAccessor _context, IUserService userService)
        {
            _interactionsService = interactionService;
            _fetchService = fetchService;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
            _userService = userService;
            _recipeService = recipeService;
            _postService = postService;
        }

        
        [HttpPost]
        public async Task<IActionResult> CreatePost(PostData postData)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

                if (postData.PostTitle.IsNullOrEmpty())
                    return BadRequest("Post Title is required");
                if (postData.PostDescription.IsNullOrEmpty())
                    return BadRequest("Post Description is required");
                if (postData.ThumbnailUrl.IsNullOrEmpty())
                    return BadRequest("Thumbnail is required");

                List<PostMedium> media = new();
                if (postData.RecipeId != null) 
                {
                    Recipe? postRecipe = await _recipeService.GetRecipeWithStepsById((int)postData.RecipeId);
                    if(postRecipe == null)
                    {
                        return BadRequest("Post recipe does not exist");
                    }
                    if(postRecipe.RecipeSteps.Count == postData.PostMediaList.Count)
                    {
                        for (int i = 0; i < postRecipe.RecipeSteps.Count; i++)
                        {
                            RecipeStep rs = postRecipe.RecipeSteps.ElementAt(i);
                            PostMediaData? md = postData.PostMediaList.Where(pm => pm.StepId == rs.StepId).FirstOrDefault();
                            if(md == null)
                            {
                                return BadRequest("Media and Recipe Steps are out of sync");
                            }

                            media.Add(new()
                            {

                                StepId = rs.StepId,
                                MediaUrl = md.MediaUrl,
                                ThumbnailUrl = md.ThumbnailUrl
                            });

                        }
                    }
                    else
                    {
                        return BadRequest("Media and Recipe Steps are out of sync");
                    }
                    postData.PostMedia = null;
                }
                else if (postData.PostMedia.IsNullOrEmpty())
                {
                    return BadRequest("Must Return Some Media");
                }



                #pragma warning disable CS8601 // Possible null reference assignment.
                Post post = new()
                {
                    PostTitle = postData.PostTitle,
                    PostDescription = postData.PostDescription,
                    UserId = currentId,
                    PostMedia = postData.PostMedia,
                    ThumbnailUrl = postData.ThumbnailUrl,
                    RecipeId = postData.RecipeId,
                    PostMediaNavigation = media,
                    PostedDatetime = DateTime.Now
                };
                #pragma warning restore CS8601 // Possible null reference assignment.

                if (await _postService.CreatePost(post))
                {
                    return Ok(new { post.PostId });
                }
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, PostData postData)
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

                Post? post = await _fetchService.GetSinglePost(postId);

                if(post == null) 
                {
                    return NotFound("Post could not be found");
                }

                if(post.UserId != currentId)
                {
                    return Unauthorized("You do not have access to update this post");
                }

                #pragma warning disable CS8601 // Possible null reference assignment.
                if (!postData.PostTitle.IsNullOrEmpty())
                    post.PostTitle = postData.PostTitle;
                if (!postData.PostDescription.IsNullOrEmpty())
                    post.PostDescription = postData.PostDescription;
                #pragma warning disable CS8601 // Possible null reference assignment.


                List<PostMedium> media = new();
                if (postData.RecipeId != null)
                {
                    Recipe? postRecipe = await _recipeService.GetRecipeWithStepsById((int)postData.RecipeId);
                    if (postRecipe == null)
                    {
                        return BadRequest("Post recipe does not exist");
                    }
                    if (postRecipe.RecipeSteps.Count == postData.PostMediaList.Count)
                    {
                        for (int i = 0; i < postRecipe.RecipeSteps.Count - 1; i++)
                        {
                            RecipeStep rs = postRecipe.RecipeSteps.ElementAt(i);
                            PostMediaData? md = postData.PostMediaList.Where(pm => pm.StepId == rs.StepId).FirstOrDefault();
                            if (md == null)
                            {
                                return BadRequest("Media and Recipe Steps are out of sync");
                            }

                            media.Add(new()
                            {
                                MediaUrl = md.MediaUrl,
                                StepId = rs.StepId
                            });

                        }
                    }
                    else
                    {
                        return BadRequest("Media and Recipe Steps are out of sync");
                    }
                    post.RecipeId = postData.RecipeId;
                    if(!await _postService.DeletePostMedia(postId))
                    {
                        return StatusCode(500, "Error removing old media");
                    }
                    post.PostMediaNavigation = media;
                    post.PostMedia = null;
                    post.ThumbnailUrl = null;
                }
                else if (!postData.PostMedia.IsNullOrEmpty() && !postData.ThumbnailUrl.IsNullOrEmpty())
                {
                    post.RecipeId = null;
                    post.PostMediaNavigation = new List<PostMedium>();
                    post.PostMedia = postData.PostMedia;
                    post.ThumbnailUrl = postData.ThumbnailUrl;
                }
                else if(!postData.PostMedia.IsNullOrEmpty() && postData.ThumbnailUrl.IsNullOrEmpty())
                {
                    return BadRequest("Must include thumbnail with Post Media");
                }

                

                

                if (await _postService.UpdatePost(post))
                {
                    return Ok();
                }
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetRecommendedPosts()
        {

            try
            {
                List<PostPreview> posts;
                if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    posts = await _fetchService.GetRecommendedPosts(currentId);
                }
                else
                {
                    posts = await _fetchService.GetRecommendedPosts();
                }

                if (posts.Count > 0)
                {
                    return Ok(posts);
                }
                else
                {
                    return StatusCode(500, "Could not generate a recommended feed. Please try reloading this page.");
                }
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpGet("following")]
        public async Task<IActionResult> GetFollowingPosts()
        {
            try
            {
                if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    List<PostPreview> posts = await _fetchService.GetFollowingPosts(currentId);
                    if (posts != null && posts.Count > 0)
                    {
                        return Ok(posts);
                    }
                    else
                    {
                        return StatusCode(500, "There was a problem with your request. Please try again.");
                    }
                }
                else
                {
                    return BadRequest("You must be logged in to view following");
                }

            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        
        [HttpGet("user/me")]
        public async Task<IActionResult> GetUserPosts()
        {
            try
            {
                if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

                List<PostPreview> posts = await _fetchService.GetUserPosts(currentId);
                if (posts != null)
                {
                    return Ok(posts);
                }
                else
                {
                    return StatusCode(500, "There was a problem with your request. Please try again.");
                }
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }
        [AllowAnonymous]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPosts(int userId)
        {
            try
            {
                List<PostPreview> posts = await _fetchService.GetUserPosts(userId);
                if (posts != null)
                {
                    return Ok(posts);
                }
                else
                {
                    return StatusCode(500, "There was a problem with your request. Please try again.");
                }
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [AllowAnonymous]
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetSinglePost(int postId)
        {
            try
            {
                Post? post = await _fetchService.GetSinglePost(postId);
                if (post != null)
                {
                    bool hasLiked;

                    if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                    {
                        //NOTE: Update front end Post model as well to reflect the two new fields

                        //only create post interaction if user is logged in
                        await _interactionsService.CreatePostInteraction(postId, currentId);
                        hasLiked = await _interactionsService.HasLiked(postId, currentId);
                    }
                    else
                    {
                        hasLiked = false;
                    }

                    object? recipeData = null;
                    string? postMedia = null;
                    if(post.Recipe == null)
                    {
                        postMedia = post.PostMedia;
                    }
                    else
                    {
                        recipeData = new
                        {
                            post.Recipe.RecipeId,
                            post.Recipe.RecipeTitle,
                            post.Recipe.RecipeDescription,
                            User = new
                            {
                                post.Recipe.User.UserId,
                                post.Recipe.User.Username,
                                post.Recipe.User.ProfilePicture
                            },
                            post.Recipe.CreatedDatetime,
                            RecipeSteps = post.Recipe.RecipeSteps.Select(rs => new
                            {
                                rs.StepId,
                                rs.StepDescription,
                                rs.StepOrder,
                                PostMedia = post.PostMediaNavigation.Where(pm => pm.StepId == rs.StepId).Select(pm => new
                                {
                                    pm.PostMediaId,
                                    pm.MediaUrl,
                                    pm.ThumbnailUrl
                                }).FirstOrDefault(),
                                StepIngredients = rs.StepIngredients.Select(si => new
                                {
                                    si.StepIngredientId,
                                    Ingredient = new 
                                    {
                                        si.Ingredient.IngredientId,
                                        si.Ingredient.IngredientDescription,
                                        si.Ingredient.IngredientTitle,
                                        si.Ingredient.IngredientIcon
                                    },
                                    si.IngredientMeasurementValue,
                                    si.IngredientMeasurementUnit
                                })
                            })
                        };
                    }

                    int likes = await _interactionsService.GetLikeCount(postId);
                    int comments = await _interactionsService.GetCommentCount(postId);

                    return Ok(new
                    {
                        post.PostId,
                        post.PostTitle,
                        post.PostDescription,
                        PostMedia = postMedia,
                        post.ThumbnailUrl,
                        User = new
                        {
                            post.User.UserId,
                            post.User.Username,
                            post.User.ProfilePicture
                        },
                        Recipe = recipeData,
                        Likes = likes,
                        Comments = comments,
                        HasLiked = hasLiked
                    });
                }
                else
                {
                    return StatusCode(500, "There was a problem with your request. Please try again.");
                }
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [AllowAnonymous]
        [HttpGet("{postId}/comments")]
        public async Task<ActionResult> GetComments(int postId)
        {
            try
            {
                List<PostComment> comments = await _interactionsService.GetComments(postId);

                if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    foreach (PostComment comment in comments)
                    {
                        BlockStatus blockStatus = await _userService.CheckBlock(currentId, comment.UserId);
                        if (blockStatus == BlockStatus.Blocked)
                        {
                            comments.Remove(comment);
                        }
                    }
                }

                //Using only the data the front end needs by making this new list, comments2
                var comments2 = new List<object>();
                comments.ForEach(comment => comments2.Add(new
                {
                    comment.CommentId,
                    comment.PostId,
                    comment.User.Username,
                    comment.User.ProfilePicture,
                    comment.Comment,
                    comment.CommentDatetime
                }));
                return Ok(comments2);
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpPost("{postId}/comments")]
        public async Task<ActionResult> PostComment(int postId, string comment)
        {
            try
            {

                if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    int numRows = await _interactionsService.PostComment(postId, currentId, comment);
                    if (numRows > 0)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest("You must be logged in to post a comment.");
                }

            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpPost("{postId}/like")]
        public async Task<ActionResult> PostLike(int postId)
        {
            try
            {
                if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    int numRows = await _interactionsService.PostLike(postId, currentId);
                    if (numRows > 0)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest("You must be logged in to like a post.");
                }

            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }

        [HttpPost("{postId}/report")]
        public async Task<IActionResult> PostReport(int postId, string message)
        {
            try
            {
                if (_claims != null && int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
                {
                    int numRows = await _interactionsService.PostReport(postId, currentId, message);
                    if (numRows > 0)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest("You must be logged in to post a report.");
                }
            }
            catch (Exception ex)
            {
                if (_claims != null && _claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
                {
                    if (ex.InnerException != null)
                    {
                        return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                    }
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
        }
    }
}
