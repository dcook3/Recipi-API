using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models.Data_Models;
using Recipi_API.Models;
using Recipi_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostInteractionsService _interactionsService;
        private readonly IPostFetchService _fetchService;
        private readonly ClaimsIdentity? _claims;
        private readonly UserService _userService;

        public PostsController(IPostInteractionsService service, IPostFetchService fetchService, IHttpContextAccessor _context, UserService userService)
        {
            _interactionsService = service;
            _fetchService = fetchService;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendedPosts()
        {

            try
            {
                List<PostPreview> posts;
                int currentId;
                if (int.TryParse(_claims.FindFirst("Id")?.Value, out currentId))
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("following")]
        public async Task<IActionResult> GetFollowingPosts()
        {
            try
            {
                int currentId;
                if (int.TryParse(_claims.FindFirst("Id")?.Value, out currentId))
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPosts(int userId)
        {
            try
            {
                List<PostPreview> posts = await _fetchService.GetUserPosts(userId);
                if (posts != null && posts.Count > 0)
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetSinglePost(int postId)
        {
            try
            {
                
                Post post = await _fetchService.GetSinglePost(postId);
                if (post != null)
                {
                    int currentId;
                    if (int.TryParse(_claims.FindFirst("Id")?.Value, out currentId))
                    {
                        //only create post interaction if user is logged in
                        await _interactionsService.CreatePostInteraction(postId, currentId);
                    }
                    
                    return Ok(post);
                }
                else
                {
                    return StatusCode(500, "There was a problem with your request. Please try again.");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{postId}/comments")]
        public async Task<ActionResult> GetComments(int postId)
        {
            try
            {
                List<PostComment> comments = await _interactionsService.GetComments(postId);
                if(comments.Count > 0)
                {
                    int currentId;
                    if (int.TryParse(_claims.FindFirst("Id")?.Value, out currentId))
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
                    else
                    {
                        return BadRequest("Must be logged in to see comments.");
                    }
                    
                    return Ok(comments);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpPost("{postId}/comments")]
        public async Task<ActionResult> PostComment(int postId, string comment)
        {
            try
            {
                int currentId;
                if (int.TryParse(_claims.FindFirst("Id")?.Value, out currentId))
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpPost("{postId}/like")]
        public async Task<ActionResult> PostLike(int postId)
        {
            try
            {
                int currentId;
                if (int.TryParse(_claims.FindFirst("Id")?.Value, out currentId))
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpPost("{postId}/report")]
        public async Task<IActionResult> PostReport(int postId, string message)
        {
            try
            {
                int currentId;
                if (int.TryParse(_claims.FindFirst("Id")?.Value, out currentId))
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }
    }
}
