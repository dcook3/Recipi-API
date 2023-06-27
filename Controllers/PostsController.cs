using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipiDBlib.Models;
using RecipiDBlib;

namespace Recipi_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetPostComments(int postId)
        {
            try
            {
                List<PostComment> comments = DatabaseFunctions.GetPostComments(postId);
                return Ok(comments);
            } 
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpPost]
        public IActionResult PostComment(int postId, string comment)
        {
            try
            {
                PostComment postComment = new PostComment();
                postComment.PostId = postId;
                postComment.Comment = comment;
                postComment.CommentDatetime = DateTime.Now;
                postComment.UserId = -1; //This is a placeholder until we have user routes with proper authentication. We'll need a function to return the HttpContext current user using our own User class.
                return Ok(postComment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult LikePost(int postId, int userId) 
        {
            try
            {
                PostLike pl = new PostLike();
                pl.PostId = postId;
                pl.UserId = userId;
                DatabaseFunctions.LikePost(pl);
                return Ok();
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult ReportPost(int postId)
        {
            try
            {
                PostReport postReport = new PostReport();
                postReport.PostId = postId;
                postReport.ReportedDatetime = DateTime.Now;
                postReport.UserId = -1; //Same placeholder reasoning as above.
                DatabaseFunctions.AddPostReport(postReport);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
