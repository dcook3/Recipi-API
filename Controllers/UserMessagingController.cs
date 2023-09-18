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

        [AllowAnonymous]
        [HttpGet("connect/{conversationId}")]
        public async Task EstablishSocketConnection(int conversationId)
        {
            if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
            {
                return;
            }

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await socketHandler.HandleConnection(currentId, conversationId, webSocket, socketService);
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
                if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

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

        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetConversationByUserId(int userId)
        {
            try
            {
                if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

                List<Conversation> convos;
                convos = await userMsging.GetConversations(currentId);

                foreach (var convo in convos)
                {
                    if (convo.UserId1 == currentId && convo.UserId2 == userId)
                    {
                        return Ok(convo);
                    }
                    if (convo.UserId2 == userId && convo.UserId1 == userId)
                    {
                        return Ok(convo);
                    }
                }
                return NotFound();
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
                if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

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
                if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

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

        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            try
            {
                if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

                List<Message> convos;
                convos = await userMsging.GetMessagesFromConversation(conversationId);

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

        [HttpPost("messages")]
        public async Task<IActionResult> CreateMessage(int conversationId, string messageContents)
        {
            try
            {
                if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

                Message msg = new()
                {
                    SendingUserId = currentId,
                    ConversationId = conversationId,
                    MessageContents = messageContents,
                    SentDatetime = DateTime.UtcNow,
                };
                if (await userMsging.CreateMessage(msg))
                {
                    return Ok(new { msg.MessageId });
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
        
        [HttpDelete("messages")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                if (claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int currentId))
                {
                    return BadRequest();
                }

                if (await userMsging.DeleteMessage(messageId))
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
    }
}
