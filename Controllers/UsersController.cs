using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        private readonly UserService userSvc;
        private readonly IConfiguration configuration;
        public UsersController(UserService _userSvc, IConfiguration _configuration, IHttpContextAccessor _context)
        {
            this.claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
            this.userSvc = _userSvc;
            this.configuration = _configuration;
        }

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

            if (user.UserRoles.Count() != 1)
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

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(tokenString);
        }

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

        

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {

            var username = this.claims?.FindFirst("Username")?.Value;
            if (this.claims == null || username.IsNullOrEmpty())
            {
                return BadRequest();
            }

            var user = await userSvc.GetUser(username);

            if (user == null)
            {
                return StatusCode(500);
            }

            return Ok(new
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                Biography = user.Biography,
                RegisteredDateTime = user.RegisteredDatetime,
                Verified = user.Verified
            });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpGet("/username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            int selfUserId;
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out selfUserId))
            {
                return BadRequest();
            }

            User? foundUser = await userSvc.GetUser(username);
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

            return Ok(new
            {
                UserId = foundUser.UserId,
                Username = foundUser.Username,
                Email = foundUser.Email,
                ProfilePicture = foundUser.ProfilePicture,
                Biography = foundUser.Biography,
                RegisteredDateTime = foundUser.RegisteredDatetime
            });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpGet("/id/{userId}")]
        public async Task<IActionResult> GetUserByUserId(int userId)
        {
            int selfUserId;
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out selfUserId))
            {
                return BadRequest();
            }

            User? foundUser = await userSvc.GetUser(userId);
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

            return Ok(new
            {
                UserId = foundUser.UserId,
                Username = foundUser.Username,
                Email = foundUser.Email,
                ProfilePicture = foundUser.ProfilePicture,
                Biography = foundUser.Biography,
                RegisteredDateTime = foundUser.RegisteredDatetime
            });
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpPut("Friend/{recievingUserId}")]
        public async Task<IActionResult> Friend(int recievingUserId)
        {
            int userId;
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out userId))
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

            if (rels.Contains("friend"))
            {
                if(await userSvc.RemoveFriend(userId, recievingUserId))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
            }

            if(await userSvc.AddFriend(userId, recievingUserId))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpGet("Friends")]
        public async Task<IActionResult> GetFriends()
        {
            int userId;
            if (this.claims == null || !int.TryParse(claims.FindFirst("Username")?.Value, out userId))
            {
                return BadRequest();
            }

            var friends = await userSvc.GetFriends(userId);

            return Ok(friends);
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpPut("Follow/{recievingUserId}")]
        public async Task<IActionResult> Follow(int recievingUserId)
        {
            int userId;
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out userId))
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

            if (rels.Contains("follow"))
            {
                if (await userSvc.UnfollowUser(userId, recievingUserId))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
            }

            if (await userSvc.FollowUser(userId, recievingUserId))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpGet("Followers")]
        public async Task<IActionResult> GetFollowers()
        {
            int userId;
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out userId))
            {
                return BadRequest();
            }

            var followers = await userSvc.GetFollowers(userId);

            return Ok(followers);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpGet("Following")]
        public async Task<IActionResult> GetFollowing()
        {
            int userId;
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out userId))
            {
                return BadRequest();
            }

            var following = await userSvc.GetFollowing(userId);

            return Ok(following);
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [HttpPut("Block/{recievingUserId}")]
        public async Task<IActionResult> Block(int recievingUserId)
        {
            int userId;
            if (this.claims == null || !int.TryParse(claims.FindFirst("Id")?.Value, out userId))
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
                if (await userSvc.UnblockUser(userId, recievingUserId))
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



    }
}
