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
    [AllowAnonymous]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly UserService userSvc;
        private readonly IConfiguration configuration;
        public UsersController(UserService _userSvc, IConfiguration _configuration)
        {
            this.userSvc = _userSvc;
            this.configuration = _configuration;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            User? user = await userSvc.GetUser(username);
            if(user != null)
            {
                return Ok(new
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture,
                    Biography = user.Biography,
                    RegisteredDateTime = user.RegisteredDatetime
                });
            }
            else
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLogin login)
        {
            if(string.IsNullOrEmpty(login.Credential) || string.IsNullOrEmpty(login.Password)) 
            {
                return BadRequest("Username and Password are Required");
            }

            User? user = await userSvc.AuthenticateLogin(login);

            if(user == null) 
            {
                return Unauthorized();
            }

            if(user.UserRoles.Count() != 1)
            {
                return StatusCode(500);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
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


        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegistration registration)
        {
            if(await userSvc.CheckUsername(registration.Username))
            {
                return Conflict("Username has Been Taken");
            }

            if(await userSvc.CheckEmail(registration.Email))
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
    }
}
