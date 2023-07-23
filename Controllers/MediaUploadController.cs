using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Recipi_API.Models.Data_Models;
using Recipi_API.Models;
using Recipi_API.Services;

namespace Recipi_API.Controllers
{
    [AllowAnonymous]
    [Route("/api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    public class MediaUploadController : ControllerBase
    {
        private readonly IPostInteractionsService _interactionsService;
        private readonly IPostFetchService _fetchService;
        private readonly ClaimsIdentity? _claims;
        private readonly IUserService _userService;

        private readonly string AWS_ACCESS_KEY_ID;
        private readonly string AWS_SECRET_ACCESS_KEY;

        private readonly string bucketName;
        private readonly string key;
        private readonly RegionEndpoint? bucketRegion;
        private readonly AmazonS3Client client;

        public MediaUploadController(IPostInteractionsService service, IPostFetchService fetchService, IHttpContextAccessor _context, IUserService userService)
        {
            _interactionsService = service;
            _fetchService = fetchService;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
            _userService = userService;

            bucketName = "recipi-pwa-storage";
            key = Guid.NewGuid().ToString() + ".txt";
            RegionEndpoint bucketRegion = RegionEndpoint.USEast2;

            AWS_ACCESS_KEY_ID = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "Invalid Keys";
            AWS_SECRET_ACCESS_KEY = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? "Invalid Keys";
            client = new AmazonS3Client(AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, bucketRegion);
        }

        [HttpPost]
        public async Task<ActionResult> GetSignedURL(SignedUrlRequest createSignedUrlRequest)
        {
            if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
            {
                return BadRequest("Must be logged in to request an upload URL.");
            }

            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    ContentBody = createSignedUrlRequest.Content
                };

                PutObjectResponse putObjectResponse = await client.PutObjectAsync(putRequest);

                GetPreSignedUrlRequest preSignedUrlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddHours(createSignedUrlRequest.TimeToLiveInHours)
                };

                string preSignedUrl = client.GetPreSignedURL(preSignedUrlRequest);

                return Ok(preSignedUrl);
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
