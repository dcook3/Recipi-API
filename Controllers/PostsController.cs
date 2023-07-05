using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models.Data_Models;
using Recipi_API.Models;
using Microsoft.EntityFrameworkCore;
using Recipi_API.Services;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostInteractionsService _interactionsService;

        public PostsController(IPostInteractionsService service)
        {
            _interactionsService = service;
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
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
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
                if (ex.InnerException != null)
                {
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
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
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
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
                    return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }
    }
}
