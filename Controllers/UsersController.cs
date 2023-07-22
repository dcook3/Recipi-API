using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recipi_API.Models;
using Recipi_API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ClaimsIdentity? claims;
        private readonly IUserService userSvc;
        private readonly IConfiguration configuration;
        public UsersController(IUserService _userSvc, IConfiguration _configuration, IHttpContextAccessor _context)
        {
            this.claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
            this.userSvc = _userSvc;
            this.configuration = _configuration;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLogin login)
        {
            if (string.IsNullOrEmpty(login.Credential) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Username and Password are Required");
            }

            User? user = await userSvc.AuthenticateLogin(login);

            if (user == null)
            {
                return Unauthorized();
            }

            if (user.UserRoles.Count != 1)
            {
                return StatusCode(500);
            }

            var claims = new[]
            {
                new Claim("Id", user.UserId.ToString()),
                new Claim("Username", user.Username),
                new Claim("Email", user.Email),
                new Claim(ClaimTypes.Role, user.UserRoles.Last().Role.RoleName)
            };

#pragma warning disable CS8604 // Possible null reference argument.
            var token = new JwtSecurityToken
            (
                issuer: this.configuration["Jwt:Issuer"],
                audience: this.configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(60),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                        SecurityAlgorithms.HmacSha256)
            );
#pragma warning restore CS8604 // Possible null reference argument.

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(tokenString);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegistration registration)
        {
            if (await userSvc.CheckUser(registration.Username))
            {
                return Conflict("Username has Been Taken");
            }

            if (await userSvc.CheckEmail(registration.Email))
            {
                return Conflict("Email is already in use, Try signing in.");
            }

            if (await userSvc.CreateUser(registration))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateProfile(UserProfileUpdate updates)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            User? user = await userSvc.GetUser(userId);
            if (user == null)
            {
                return NotFound();
            }

#pragma warning disable CS8604 // Possible null reference argument. checks for IsNullOrEmpty()
            if (!updates.Username.IsNullOrEmpty() && await userSvc.CheckUser(updates.Username))
            {
                return Conflict("Username is already in use");
            }
#pragma warning restore CS8604 // Possible null reference argument.


            if (await userSvc.UpdateUserProfile(updates, user))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }


        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {

            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            var user = await userSvc.GetUser(userId);

            if (user == null)
            {
                return StatusCode(500);
            }

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.ProfilePicture,
                user.Biography,
                user.RegisteredDatetime,
                user.Verified
            });
        }
       
        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int selfUserId))
            {
                return BadRequest();
            }

            User? foundUser = await userSvc.GetUser(username, selfUserId);
            if(foundUser == null)
            {
                return NotFound();
            }

            var blockStatus = await userSvc.CheckBlock(selfUserId, foundUser.UserId);
            if ((int)blockStatus > 0)
            {
                if (blockStatus == BlockStatus.Blocked)
                {
                    return Unauthorized("User has been blocked");
                }
                else
                {
                    return NotFound();
                }
            }

            List<string> combinedRels = new();
            combinedRels = combinedRels.Concat(foundUser.UserRelationshipInitiatingUsers.Select(rel => rel.Relationship).ToList())
                                       .Concat(foundUser.UserRelationshipReceivingUsers.Select(rel => rel.Relationship).ToList())
                                       .ToList();

            return Ok(new
            {
                foundUser.UserId,
                foundUser.Username,
                foundUser.Email,
                foundUser.ProfilePicture,
                foundUser.Biography,
                foundUser.RegisteredDatetime,
                YourRelationships = combinedRels
            });
        }
        
        [HttpGet("id/{userId}")]
        public async Task<IActionResult> GetUserByUserId(int userId)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int selfUserId))
            {
                return BadRequest();
            }

            User? foundUser = await userSvc.GetUser(userId, selfUserId);
            if (foundUser == null)
            {
                return NotFound();
            }

            var blockStatus = await userSvc.CheckBlock(selfUserId, foundUser.UserId);
            if ((int)blockStatus > 0)
            {
                if ((int)blockStatus == 2)
                {
                    return Unauthorized("User has been blocked");
                }
                else
                {
                    return NotFound();
                }
            }

            List<string> combinedRels = new();
            combinedRels = combinedRels.Concat(foundUser.UserRelationshipInitiatingUsers.Select(rel => rel.Relationship).ToList())
                                       .Concat(foundUser.UserRelationshipReceivingUsers.Select(rel => rel.Relationship).ToList())
                                       .ToList();
            return Ok(new
            {
                foundUser.UserId,
                foundUser.Username,
                foundUser.Email,
                foundUser.ProfilePicture,
                foundUser.Biography,
                foundUser.RegisteredDatetime,
                YourRelationships = combinedRels
            });
        }
        
        



        
        [HttpGet("Friend")]
        public async Task<IActionResult> GetFriends()
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            var friends = await userSvc.GetFriends(userId);
            return Ok(friends.Select(user =>
                new
                {
                    user.UserId,
                    user.Username,
                    user.Biography,
                    user.ProfilePicture
                }
            ));
        }
        
        [HttpGet("Friend/Requests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            var friendRequests = await userSvc.GetFriendRequests(userId);
            return Ok(friendRequests.Select(user =>
                new
                {
                    user.UserId,
                    user.Username,
                    user.Biography,
                    user.ProfilePicture
                }
            ));
        }
        
        [HttpPost("Friend/Request/{recievingUserId}")]
        public async Task<IActionResult> FriendRequest(int recievingUserId)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            if (!await userSvc.CheckUser(recievingUserId))
            {
                return NotFound("User Not Found");
            }

            var blockStatus = await userSvc.CheckBlock(userId, recievingUserId);
            if((int) blockStatus > 0)
            {
                if((int) blockStatus == 2)
                {
                    return Unauthorized("User has been blocked");
                }
                else
                {
                    return NotFound();
                }
            }

            var rels = await userSvc.GetRelationships(userId, recievingUserId);
            var friendRequest = rels.Where(rels => rels.Relationship == "friendRequest").FirstOrDefault();

            if(friendRequest != null)
            {
                return Conflict("A Friend Request has already been sent");
            }

            if(await userSvc.RequestFriend(userId, recievingUserId))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }
        
        [HttpPost("Friend/Accept/{recievingUserId}")]
        public async Task<IActionResult> AcceptFriend(int recievingUserId)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            if (!await userSvc.CheckUser(recievingUserId))
            {
                return NotFound("User Not Found");
            }

            var blockStatus = await userSvc.CheckBlock(userId, recievingUserId);
            if ((int)blockStatus > 0)
            {
                if ((int)blockStatus == 2)
                {
                    return Unauthorized("User has been blocked");
                }
                else
                {
                    return Unauthorized("You have been blocked by user");
                }
            }

            var rels = await userSvc.GetRelationships(userId, recievingUserId);
            var friendRequest = rels.Where(rels => rels.Relationship == "friendRequest" && rels.ReceivingUserId == userId).FirstOrDefault();

            if (friendRequest == null)
            {
                return NotFound("Friend request can't be found");
            }

            if (await userSvc.AcceptFriend(friendRequest))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }
        
        [HttpDelete("Friend/Deny/{recievingUserId}")]
        public async Task<IActionResult> DenyFriend(int recievingUserId)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            if (!await userSvc.CheckUser(recievingUserId))
            {
                return NotFound("User Not Found");
            }

            var blockStatus = await userSvc.CheckBlock(userId, recievingUserId);
            if ((int)blockStatus > 0)
            {
                if ((int)blockStatus == 2)
                {
                    return Unauthorized("User has been blocked");
                }
                else
                {
                    return Unauthorized("You have been blocked by user");
                }
            }

            var rels = await userSvc.GetRelationships(userId, recievingUserId);
            var friendRequest = rels.Where(rels => rels.Relationship == "friendRequest").FirstOrDefault();

            if (friendRequest == null)
            {
                return NotFound("Friend request can't be found");
            }

            if (await userSvc.RemoveRelationship(friendRequest))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }
        
        [HttpDelete("Friend/Remove/{recievingUserId}")]
        public async Task<IActionResult> RemoveFriend(int recievingUserId)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            if (!await userSvc.CheckUser(recievingUserId))
            {
                return NotFound("User Not Found");
            }

            var blockStatus = await userSvc.CheckBlock(userId, recievingUserId);
            if ((int)blockStatus > 0)
            {
                if ((int)blockStatus == 2)
                {
                    return Unauthorized("User has been blocked");
                }
                else
                {
                    return Unauthorized("You have been blocked by user");
                }
            }

            var rels = await userSvc.GetRelationships(userId, recievingUserId);
            var friend = rels.Where(rels => rels.Relationship == "friend").FirstOrDefault();

            if (friend == null)
            {
                return NotFound("Friend request can't be found");
            }

            if (await userSvc.RemoveRelationship(friend))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }


        
        [HttpPost("Follow/{recievingUserId}")]
        public async Task<IActionResult> Follow(int recievingUserId)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            if (!await userSvc.CheckUser(recievingUserId))
            {
                return NotFound("User Not Found");
            }

            var blockStatus = await userSvc.CheckBlock(userId, recievingUserId);
            if ((int)blockStatus > 0)
            {
                if ((int)blockStatus == 2)
                {
                    return Unauthorized("User has been blocked");
                }
                else
                {
                    return NotFound();
                }
            }

            var rels = await userSvc.GetRelationships(userId, recievingUserId);
            var following = rels.Where(rels => rels.Relationship == "follow" &&
                                                rels.InitiatingUserId == userId).FirstOrDefault();

            if (following == null)
            {
                if (await userSvc.FollowUser(userId, recievingUserId))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
            }

            if (await userSvc.RemoveRelationship(following))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }
        
        [HttpGet("Followers")]
        public async Task<IActionResult> GetFollowers()
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            var followers = await userSvc.GetFollowers(userId);

            return Ok(followers.Select(user =>
                new
                {
                    user.UserId,
                    user.Username,
                    user.Biography,
                    user.ProfilePicture
                }
            ));
        }
        
        [HttpGet("Following")]
        public async Task<IActionResult> GetFollowing()
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            var following = await userSvc.GetFollowing(userId);

            return Ok(following.Select(user =>
                new
                {
                    user.UserId,
                    user.Username,
                    user.Biography,
                    user.ProfilePicture
                }
            ));
        }
        

        [HttpPost("Block/{recievingUserId}")]
        public async Task<IActionResult> Block(int recievingUserId)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            if (!await userSvc.CheckUser(recievingUserId))
            {
                return NotFound("User Not Found");
            }

            var blockStatus = await userSvc.CheckBlock(userId, recievingUserId);
            
            if ((int)blockStatus == 2)
            {
                var rels = await userSvc.GetRelationships(userId, recievingUserId);
                var blocking = rels.Where(rels => rels.Relationship == "block" &&
                                                    rels.InitiatingUserId == userId).FirstOrDefault();
                if (blocking != null && await userSvc.RemoveRelationship(blocking))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
            }

            if (await userSvc.BlockUser(userId, recievingUserId))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }
        

        [HttpPost("/BugReport")]
        public async Task<IActionResult> ReportBug(string message)
        {
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
            {
                return BadRequest();
            }

            if (message.IsNullOrEmpty())
            {
                return BadRequest("Message is required");
            }

            if(await userSvc.CreateBugReport(userId, message))
            {
                return Ok();
            }

            return StatusCode(500);
        }
        
    }
}
