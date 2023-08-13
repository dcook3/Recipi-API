using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Recipi_API.Models;
using Recipi_API.Services;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    public class MediaUploadController : ControllerBase
    {
        private readonly ClaimsIdentity? _claims;

        private readonly string AWS_ACCESS_KEY_ID;
        private readonly string AWS_SECRET_ACCESS_KEY;

        private readonly string bucketName;
        private readonly string key;
        private readonly RegionEndpoint? bucketRegion;
        private readonly AmazonS3Client client;

        public MediaUploadController(IHttpContextAccessor _context)
        {
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;

            bucketName = "recipi-pwa-storage";
            key = Guid.NewGuid().ToString() + ".txt";
            RegionEndpoint bucketRegion = RegionEndpoint.USEast2;

            AWS_ACCESS_KEY_ID = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "Invalid Keys";
            AWS_SECRET_ACCESS_KEY = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? "Invalid Keys";
            client = new AmazonS3Client(AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, bucketRegion);
        }

        [HttpGet]
        public async Task<ActionResult> GetSignedURL()
        {
            if (_claims == null || !int.TryParse(_claims.FindFirst("Id")?.Value, out int currentId))
            {
                return BadRequest("Must be logged in to request an upload URL.");
            }

            try
            {
                GetPreSignedUrlRequest preSignedUrlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddHours(1)
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
