using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models;
using Microsoft.EntityFrameworkCore;
using Recipi_API.Services;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Recipi_API.Controllers
{
    [Route("api/[controller]")]
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
                    return BadRequest(ex.InnerException.ToString());
                }
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{postId}/comments")]
        public async Task<ActionResult> PostComment(int postId, int userId, string comment)
        {
            try
            {
                int numRows = await _interactionsService.PostComment(postId, userId, comment);
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
        public async Task<IActionResult> PostReport(int postId, int userId, string message)
        {
            try
            {
                int numRows = await _interactionsService.PostReport(postId, userId, message);
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
