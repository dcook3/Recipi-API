using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models;
using Microsoft.EntityFrameworkCore;
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

        public PostsController(IPostInteractionsService service, IHttpContextAccessor _context, UserService userService)
        {
            _interactionsService = service;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
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
        public async Task<ActionResult> PostComment(PostCommentData postComment)
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
