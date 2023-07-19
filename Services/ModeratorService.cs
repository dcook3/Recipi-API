using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;
using System.Security.Claims;

namespace Recipi_API.Services
{
    public class ModeratorService : IModeratorService
    {
        private readonly RecipiDbContext context = new();
        public async Task<int> ChangeReportStatus(int reportId, string changedStatus)
        {
            PostReport postReport = await context.PostReports.FirstAsync(p => p.PostReportId == reportId);
            postReport.Status = changedStatus;
            context.Update(postReport);
            return await context.SaveChangesAsync();
        }

        public async Task<List<BugReport>> GetBugReports() => await context.BugReports.OrderBy(br => br.ReportedDatetime).ToListAsync();
        public async Task<List<BugReport>> GetBugReports(string status) => await context.BugReports.Where(br => br.Status == status).OrderBy(br => br.ReportedDatetime).ToListAsync();
        public async Task<List<PostReport>> GetPostReports() => await context.PostReports.OrderBy(pr => pr.ReportedDatetime).ToListAsync();
        public async Task<List<PostReport>> GetPostReports(string status) => await context.PostReports.Where(pr => pr.Status == status).OrderBy(pr => pr.ReportedDatetime).ToListAsync();
    }
}
