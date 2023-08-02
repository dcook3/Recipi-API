using Recipi_API.Models;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Services
{
    public interface IUserService
    {
        Task<bool> AcceptFriend(UserRelationship rel);
        Task<User?> AuthenticateLogin(UserLogin login);
        Task<bool> BlockUser(int initiatingUserId, int recievingUserId);
        Task<BlockStatus> CheckBlock(int selfUserId, int otherUserId);
        Task<bool> CheckEmail(string email);
        Task<bool> CheckUser(int userId);
        Task<bool> CheckUser(string username);
        Task<bool> CreateBugReport(int userId, string message);
        Task<bool> CreateUser(UserRegistration registration);
        Task<bool> FollowUser(int initiatingUserId, int recievingUserId);
        Task<UserStats> GetUserStats(int id);
        Task<List<User>> GetFollowers(int userId);
        Task<List<User>> GetFollowing(int userId);
        Task<List<User>> GetFriendRequests(int userId);
        Task<List<User>> GetFriends(int userId);
        Task<List<UserRelationship>> GetRelationships(int userId1, int userId2);
        Task<List<string>> GetRelationshipStrings(int userId1, int userId2);
        Task<Role?> GetRole(string roleName);
        Task<User?> GetUser(int id);
        Task<User?> GetUser(int id, int selfUserId);
        Task<User?> GetUser(string username);
        Task<User?> GetUser(string username, int selfUserId);
        Task<bool> RemoveRelationship(UserRelationship rel);
        Task<bool> RequestFriend(int initiatingUserId, int recievingUserId);
        Task<bool> UpdateUserProfile(UserProfileUpdate updates, User user);
    }
}