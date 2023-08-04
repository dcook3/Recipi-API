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
            try
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

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegistration registration)
        {
            try
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
            try
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
                var userStats = await userSvc.GetUserStats(userId);


                return Ok(new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.ProfilePicture,
                user.Biography,
                user.RegisteredDatetime,
                user.Verified,
                UserStats = userStats
            });
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
       
        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                User? foundUser;
                if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int selfUserId))
                {
                    foundUser = await userSvc.GetUser(username);
                    if (foundUser == null)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    foundUser = await userSvc.GetUser(username, selfUserId);
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
                }


                List<string> combinedRels = new List<string>();
                combinedRels = combinedRels.Concat(foundUser.UserRelationshipInitiatingUsers.Select(rel => (rel.Relationship == "friend") ? "friend" : (rel.Relationship + "ed")).ToList())
                                           .Concat(foundUser.UserRelationshipReceivingUsers.Select(rel => (rel.Relationship == "friend") ? "friend" : (rel.Relationship + "ing")).ToList())
                                           .ToList();

                var userStats = await userSvc.GetUserStats(foundUser.UserId);

                return Ok(new
                {
                    foundUser.UserId,
                    foundUser.Username,
                    foundUser.Email,
                    foundUser.ProfilePicture,
                    foundUser.Biography,
                    foundUser.RegisteredDatetime,
                    YourRelationships = combinedRels,
                    UserStats = new
                    {
                        userStats.following,
                        userStats.followers,
                        userStats.posts
                    }
                });
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

        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserByUserId(int userId)
        {
            try
            {
                User? foundUser;
                List<string> combinedRels = new List<string>();
                if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int selfUserId))
                {
                    foundUser = await userSvc.GetUser(userId);
                    if (foundUser == null)
                    {
                        return NotFound();
                    }
                }
                else{
                    foundUser = await userSvc.GetUser(userId, selfUserId);
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
                    combinedRels = combinedRels.Concat(foundUser.UserRelationshipInitiatingUsers.Select(rel => (rel.Relationship == "friend") ? "friend" : (rel.Relationship + "ed")).ToList())
                                           .Concat(foundUser.UserRelationshipReceivingUsers.Select(rel => (rel.Relationship == "friend") ? "friend" : (rel.Relationship + "ing")).ToList())
                                           .ToList();
                }

                var userStats = await userSvc.GetUserStats(foundUser.UserId);

                return Ok(new
                {
                    UserId = foundUser.UserId,
                    Username = foundUser.Username,
                    Email = foundUser.Email,
                    ProfilePicture = foundUser.ProfilePicture,
                    Biography = foundUser.Biography,
                    RegisteredDateTime = foundUser.RegisteredDatetime,
                    YourRelationships = combinedRels,
                    UserStats = new
                    {
                        userStats.following,
                        userStats.followers,
                        userStats.posts
                    }
                });
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


        
        [HttpGet("Friend")]
        public async Task<IActionResult> GetFriends()
        {
            try
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
        
        [HttpGet("Friend/Requests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            try
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
        
        [HttpPost("Friend/Request/{recievingUserId}")]
        public async Task<IActionResult> FriendRequest(int recievingUserId)
        {
            try
            {
                  if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out int userId))
                  {
                      return BadRequest();
                  }

                  if(userId == recievingUserId)
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
                var friendRequest = rels.Where(rels => rels.Relationship == "friendRequest").FirstOrDefault();

                if (friendRequest != null)
                {
                    return Conflict("A Friend Request has already been sent");
                }

                if (await userSvc.RequestFriend(userId, recievingUserId))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
                
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
        
        [HttpPost("Friend/Accept/{recievingUserId}")]
        public async Task<IActionResult> AcceptFriend(int recievingUserId)
        {
            try
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
        
        [HttpDelete("Friend/Deny/{recievingUserId}")]
        public async Task<IActionResult> DenyFriend(int recievingUserId)
        {
            try
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
        
        [HttpDelete("Friend/Remove/{recievingUserId}")]
        public async Task<IActionResult> RemoveFriend(int recievingUserId)
        {
            try
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


        
        [HttpPost("Follow/{recievingUserId}")]
        public async Task<IActionResult> Follow(int recievingUserId)
        {
            try
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

        [AllowAnonymous]
        [HttpGet("{userId}/Followers")]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            try
            {
                if (int.TryParse(claims.FindFirst("Id")?.Value, out int selfId))
                {
                    var blockStatus = await userSvc.CheckBlock(selfId, userId);
                    if ((int)blockStatus > 0)
                    {
                        if ((int)blockStatus == 2)
                        {
                            return Unauthorized("User has been blocked");
                        }
                        else
                        {
                            return Ok(new List<User>());
                        }
                    }
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

        [AllowAnonymous]
        [HttpGet("{userId}/Following")]
        public async Task<IActionResult> GetFollowing(int userId)
        {
            try
            {
                if (int.TryParse(claims.FindFirst("Id")?.Value, out int selfId))
                {
                    var blockStatus = await userSvc.CheckBlock(selfId, userId);
                    if ((int)blockStatus > 0)
                    {
                        if ((int)blockStatus == 2)
                        {
                            return Unauthorized("User has been blocked");
                        }
                        else
                        {
                            return Ok(new List<User>());
                        }
                    }
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
        

        [HttpPost("Block/{recievingUserId}")]
        public async Task<IActionResult> Block(int recievingUserId)
        {
            try
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
                if(await userSvc.BlockUser(userId, recievingUserId))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
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
        

        [HttpPost("BugReport")]
        public async Task<IActionResult> ReportBug(string message)
        {
            try
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
