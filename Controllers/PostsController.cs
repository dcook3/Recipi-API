using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models;
using Microsoft.EntityFrameworkCore;
using Recipi_API.Services;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Diagnostics.Eventing.Reader;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{postId}/comments")]
        public async Task<ActionResult> PostComment(int postId, string comment)
        {
            try
            {
                int numRows = await _interactionsService.PostComment(postComment.PostId, postComment.UserId, postComment.Comment);
                if(numRows > 0)
                {
                    return Ok();
                }
                else 
                {
                    return BadRequest(); 
                }
            }
            catch (Exception ex)
            {
                if(ex.InnerException != null)
                {
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("{postId}/like")]
        public async Task<ActionResult> PostLike(PostLikeData like)
        {
            try
            {
                int numRows = await _interactionsService.PostLike(like.PostId, like.UserId);
                if (numRows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("{postId}/report")]
        public async Task<IActionResult> PostReport(PostReportData report)
        {
            try
            {
                int numRows = await _interactionsService.PostReport(report.PostId, report.UserId, report.Message);
                if (numRows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
            }
        }
    }
}
