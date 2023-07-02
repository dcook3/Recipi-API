using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models;
using Microsoft.EntityFrameworkCore;
using Recipi_API.Services;

namespace Recipi_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly PostInteractionsService _interactionsService;

        public PostsController(PostInteractionsService service)
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
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("{postId}/comments")]
        public async Task<ActionResult> PostComment(int postId, string comment)
        {
            try
            {
                int numRows = await _interactionsService.PostComment(postId, comment);
                if(numRows > 0) 
                {
                    return Ok();
                }
                else 
                {
                    return BadRequest(); 
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
        [HttpPost("{postId}/like")]
        public async Task<ActionResult> PostLike(int postId, int userId)
        {
            try
            {
                int numRows = await _interactionsService.PostLike(postId, userId);
                if (numRows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
        [HttpPost("{postId}/report")]
        public async Task<IActionResult> PostReport(int postId, string message)
        {
            try
            {
                int numRows = await _interactionsService.PostReport(postId, message);
                if (numRows > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
