using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net.WebSockets;

using Recipi_API.Models.Data_Models;
using Recipi_API.Models;
using Recipi_API.Services;
using System.Text;

namespace Recipi_API.Controllers
{
    [AllowAnonymous] //Don't forget to remove this
    [Route("/api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]

    public class UserMessagingController : ControllerBase
    {
        private readonly ClaimsIdentity? claims;
        private readonly UserMessagingSocketService socketService;
        private readonly IUserMessagesService userMsging;
        private readonly IWebSocketHandler socketHandler;

        public UserMessagingController(IHttpContextAccessor _context, IUserMessagesService _userMsging, UserMessagingSocketService _socketService, IWebSocketHandler _socketHandler)
        {
            claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
            userMsging = _userMsging;
            socketService = _socketService;
            socketHandler = _socketHandler;
        }

        [HttpGet("connect")]
        public async Task EstablishSocketConnection()
        {
            if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
            { //Don't forget to remove this comment
                //return BadRequest();
            }

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await socketHandler.HandleConnection(Guid.NewGuid(), webSocket, socketService);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                int currentId = 7;
                //if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                //{ //Don't forget to remove this
                //    //return BadRequest();
                //}

                List<Conversation> convos;
                convos = await userMsging.GetConversations(currentId);

                if (convos.Count > 0)
                {
                    return Ok(convos);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                if (claims != null && claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
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

        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation(int userId)
        {
            try
            {
                int currentId = 7;
                //if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                //{
                //    return BadRequest();
                //}

                Conversation convo = new()
                {
                    UserId1 = currentId,
                    UserId2 = userId,
                    Messages = new List<Message>(),
                };
                if (await userMsging.CreateConversation(convo))
                {
                    return Ok(new { convo.ConversationId });
                }
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                if (claims != null && claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
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
        
        [HttpDelete("conversations")]
        public async Task<IActionResult> DeleteConversation(int conversationId)
        {
            try
            {
                int currentId = 7;
                //if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                //{
                //    return BadRequest();
                //}

                if (await userMsging.DeleteConversation(conversationId))
                {
                    return Ok();
                }
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                if (claims != null && claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
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

        //[HttpGet("messages")]
        //public async Task<IActionResult> GetMessages()
        //{
        //    try
        //    {
        //        int currentId = 7;
        //        //if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
        //        //{ //Don't forget to remove this
        //        //    //return BadRequest();
        //        //}

        //        List<Conversation> convos;
        //        convos = await userMsging.GetMessages(currentId);

        //        if (convos.Count > 0)
        //        {
        //            return Ok(convos);
        //        }
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (claims != null && claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
        //        {
        //            if (ex.InnerException != null)
        //            {
        //                return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
        //            }
        //            return StatusCode(500, ex.Message);
        //        }
        //        else
        //        {
        //            return StatusCode(500, "Internal server error. Please try again later.");
        //        }
        //    }
        //}

        //[HttpPost("conversations")]
        //public async Task<IActionResult> CreateConversation(int userId)
        //{
        //    try
        //    {
        //        int currentId = 7;
        //        //if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
        //        //{
        //        //    return BadRequest();
        //        //}

        //        Conversation convo = new()
        //        {
        //            UserId1 = currentId,
        //            UserId2 = userId,
        //            Messages = new List<Message>(),
        //        };
        //        if (await userMsging.CreateConversation(convo))
        //        {
        //            return Ok(new { convo.ConversationId });
        //        }
        //        return StatusCode(500);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (claims != null && claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
        //        {
        //            if (ex.InnerException != null)
        //            {
        //                return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
        //            }
        //            return StatusCode(500, ex.Message);
        //        }
        //        else
        //        {
        //            return StatusCode(500, "Internal server error. Please try again later.");
        //        }
        //    }
        //}
        //
        //[HttpDelete("conversations")]
        //public async Task<IActionResult> DeleteConversation(int conversationId)
        //{
        //    try
        //    {
        //        int currentId = 7;
        //        //if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
        //        //{
        //        //    return BadRequest();
        //        //}

        //        if (await userMsging.DeleteConversation(conversationId))
        //        {
        //            return Ok();
        //        }
        //        return StatusCode(500);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (claims != null && claims.FindFirst(ClaimTypes.Role)!.Value == "Developer")
        //        {
        //            if (ex.InnerException != null)
        //            {
        //                return StatusCode(500, ex.InnerException.Message + "\n" + ex.Message);
        //            }
        //            return StatusCode(500, ex.Message);
        //        }
        //        else
        //        {
        //            return StatusCode(500, "Internal server error. Please try again later.");
        //        }
        //    }
        //}
    }
}
