using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IModeratorService
    {
        public Task<List<PostReport>> GetPostReports();
        public Task<List<PostReport>> GetPostReports(string status);
        //Consider what this might look like on the UI before continuing with a full table read
        public Task<List<BugReport>> GetBugReports();
        public Task<List<BugReport>> GetBugReports(string status);
        public Task<int> ChangeReportStatus(int reportId, string changedStatus);
    }
}
