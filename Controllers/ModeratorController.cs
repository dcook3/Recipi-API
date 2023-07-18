using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recipi_API.Models;
using Recipi_API.Services;
using System.Data;
using System.Security.Claims;

namespace Recipi_API.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class ModeratorController : ControllerBase
    {
        private readonly IModeratorService moderatorService;
        private readonly ClaimsIdentity? _claims;

        public ModeratorController(IModeratorService service, IHttpContextAccessor _context)
        {
            moderatorService = service;
            _claims = (ClaimsIdentity?)_context.HttpContext?.User?.Identity;
        }

        [HttpGet("reports/post")]
        public async Task<ActionResult> GetPostReports()
        {
            try
            {
                List<PostReport> prs = await moderatorService.GetPostReports();

                if (prs.Count >= 0)
                {
                    return Ok(prs);
                }
                return BadRequest("Please check that the post this user has reported exists.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }

        [HttpGet("reports/bug")]
        public async Task<ActionResult> GetBugReports()
        {
            try 
            { 
                List<BugReport> brs = await moderatorService.GetBugReports();

                if (brs.Count >= 0)
                {
                    return Ok(brs);
                }
                return BadRequest("Please check that the post this user has reported exists.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("reports/posts/{reportId}")]
        public async Task<ActionResult> PostReportedPostStatusChange(int reportId, string newStatus)
        {
            try
            {
                int numrows = await moderatorService.ChangeReportStatus(reportId, newStatus);
                if (numrows > 0)
                {
                    return Ok(numrows);
                }
                return BadRequest("Please ensure post report exists, and that the new status is valid.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
