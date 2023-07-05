using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models.Data_Models;
using Recipi_API.Models;
using Recipi_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    public class PostsController : ControllerBase
    {
        private readonly IPostInteractionsService _interactionsService;
        private readonly ClaimsIdentity? _claims;
        private readonly UserService _userService;

        public PostsController(IPostInteractionsService service, ClaimsIdentity claims, UserService userService)
        {
            _interactionsService = service;
            _claims = claims;
            _userService = userService;
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
                    return BadRequest("You must be logged in to post a like.");
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
