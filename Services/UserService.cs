using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;

namespace Recipi_API.Services
{
    public class UserService : IUserService
    {
        private readonly static RecipiDbContext db = new();


        public async Task<User?> GetUser(int id) => await db.Users.Where(user => user.UserId == id).FirstOrDefaultAsync();
        public async Task<User?> GetUser(int id, int selfUserId) =>
            await db.Users.Where(user => user.UserId == id)
                          .Include(user => user.UserRelationshipInitiatingUsers.Where(rel => rel.InitiatingUserId == selfUserId))
                          .Include(user => user.UserRelationshipReceivingUsers.Where(rel => rel.ReceivingUserId == selfUserId))
                    .FirstOrDefaultAsync();
        public async Task<User?> GetUser(string username) => await db.Users.Where(user => user.Username == username).FirstOrDefaultAsync();
        public async Task<User?> GetUser(string username, int selfUserId) =>
            await db.Users.Where(user => user.Username == username)
                          .Include(user => user.UserRelationshipInitiatingUsers.Where(rel => rel.InitiatingUserId == selfUserId))
                          .Include(user => user.UserRelationshipReceivingUsers.Where(rel => rel.ReceivingUserId == selfUserId))
                    .FirstOrDefaultAsync();

        public async Task<UserStats> GetUserStats(int id)
        {
            return new()
            {
                followers = await db.UserRelationships.Where(ur => ur.ReceivingUserId == id && ur.Relationship == "follow").CountAsync(),
                following = await db.UserRelationships.Where(ur => ur.InitiatingUserId == id && ur.Relationship == "follow").CountAsync(),
                friends = await db.UserRelationships.Where(ur => (ur.ReceivingUserId == id || ur.InitiatingUserId == id) && ur.Relationship == "friend").CountAsync(),
                posts = await db.Posts.Where(post => post.UserId == id).CountAsync()
            };
        }
        
        public async Task<bool> CreateUser(UserRegistration registration)
        {

            User user = new()
            {
                Username = registration.Username,
                Email = registration.Email,
                Password = registration.Password,
                Biography = registration.Biography,
                ProfilePicture = registration.ProfilePicture,
                Verified = 0,
                RegisteredDatetime = DateTime.Now
            };

            db.Users.Add(user);

            UserRole role = new()
            {
                User = user,
                RoleId = 1,
                ExpirationDays = -1,
                GrantedDatetime = DateTime.Now
            };

            db.UserRoles.Add(role);

            var rowCount = await db.SaveChangesAsync();

            return rowCount == 2;
        }

        public async Task<User?> AuthenticateLogin(UserLogin login)
        {
            return await db.Users.Where(user =>
                                       (user.Username == login.Credential || user.Email == login.Credential) &&
                                        user.Password == login.Password)
                                       .Include(user => user.UserRoles)
                                       .ThenInclude(userRole => userRole.Role)
                                       .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetRelationshipStrings(int userId1, int userId2) =>
            await db.UserRelationships.Where(rel => (rel.InitiatingUserId == userId1 || rel.InitiatingUserId == userId2) ||
                                                    (rel.ReceivingUserId == userId1 || rel.ReceivingUserId == userId2))
                                                    .Select(rel => rel.Relationship)
                                                    .ToListAsync();
        public async Task<List<UserRelationship>> GetRelationships(int userId1, int userId2) =>
            await db.UserRelationships.Where(rel => (rel.InitiatingUserId == userId1 || rel.InitiatingUserId == userId2) ||
                                                    (rel.ReceivingUserId == userId1 || rel.ReceivingUserId == userId2))
                                                    .ToListAsync();

        public async Task<bool> RequestFriend(int initiatingUserId, int recievingUserId)
        {
            db.UserRelationships.Add(new UserRelationship()
            {
                InitiatingUserId = initiatingUserId,
                ReceivingUserId = recievingUserId,
                Relationship = "friendRequest",
                InitiatedDatetime = DateTime.Now
            });
            var res = await db.SaveChangesAsync();
            return res == 1;
        }
        public async Task<bool> AcceptFriend(UserRelationship rel)
        {
            if (rel.Relationship != "friendRequest") return false;

            rel.Relationship = "friend";
            var res = await db.SaveChangesAsync();
            return res == 1;
        }

        public async Task<List<User>> GetFriends(int userId)
        {
            var friendingUser = await db.UserRelationships.Where(rel => rel.InitiatingUserId == userId && rel.Relationship == "friend")
                                                      .Select(rel => rel.ReceivingUser)
                                                      .ToListAsync();
            var userFriending = await db.UserRelationships.Where(rel => rel.ReceivingUserId == userId && rel.Relationship == "friend")
                                                      .Select(rel => rel.InitiatingUser)
                                                      .ToListAsync();

            return friendingUser.Concat(userFriending).ToList();
        }

        public async Task<List<User>> GetFriendRequests(int userId) =>
            await db.UserRelationships.Where(rel => rel.ReceivingUserId == userId && rel.Relationship == "friendRequest")
                                                      .Select(rel => rel.ReceivingUser)
                                                      .ToListAsync();

        public async Task<bool> FollowUser(int initiatingUserId, int recievingUserId)
        {
            db.UserRelationships.Add(new UserRelationship()
            {
                InitiatingUserId = initiatingUserId,
                ReceivingUserId = recievingUserId,
                Relationship = "follow",
                InitiatedDatetime = DateTime.Now
            });
            var res = await db.SaveChangesAsync();
            return res == 1;
        }

        public async Task<List<User>> GetFollowing(int userId) =>
            await db.UserRelationships.Where(rel => rel.InitiatingUserId == userId && rel.Relationship == "follow")
                                      .Select(rel => rel.ReceivingUser)
                                      .ToListAsync();
        public async Task<List<User>> GetFollowers(int userId) =>
            await db.UserRelationships.Where(rel => rel.ReceivingUserId == userId && rel.Relationship == "follow")
                                      .Select(rel => rel.InitiatingUser)
                                      .ToListAsync();

        public async Task<BlockStatus> CheckBlock(int selfUserId, int otherUserId)
        {
            var blocks = await db.UserRelationships.Where(rel => ((rel.InitiatingUserId == selfUserId || rel.InitiatingUserId == otherUserId) ||
                                                           (rel.ReceivingUserId == selfUserId || rel.ReceivingUserId == otherUserId))
                                                           && rel.Relationship == "block")
                                                    .ToListAsync();

            // neither user is blocking
            if (blocks.Count == 0)
            {
                return BlockStatus.None;
            }
            //both users are blocking
            if (blocks.Count > 1)
            {
                return BlockStatus.Both;
            }

            // user is blocking
            if (blocks[0].InitiatingUserId == selfUserId)
            {
                return BlockStatus.Blocking;
            }
            //user is being blocked
            else
            {
                return BlockStatus.Blocked;
            }
        }
        public async Task<bool> BlockUser(int initiatingUserId, int recievingUserId)
        {
            var rels = await db.UserRelationships.Where(rel => (rel.InitiatingUserId == initiatingUserId || rel.ReceivingUserId == recievingUserId)).ToListAsync();

            if (rels.Count > 0)
            {
                db.UserRelationships.RemoveRange(rels);
                await db.SaveChangesAsync();
            }

            db.UserRelationships.Add(new UserRelationship()
            {
                InitiatingUserId = initiatingUserId,
                ReceivingUserId = recievingUserId,
                Relationship = "block",
                InitiatedDatetime = DateTime.Now
            });
            var res = await db.SaveChangesAsync();
            return res == 1;
        }

        public async Task<bool> RemoveRelationship(UserRelationship rel)
        {
            db.UserRelationships.Remove(rel);
            var res = await db.SaveChangesAsync();
            return res == 1;
        }

        public async Task<bool> CheckUser(string username)
        {
            var userCount = await db.Users.Where(user => user.Username == username).CountAsync();
            return userCount > 0;
        }
        public async Task<bool> CheckUser(int userId)
        {
            var userCount = await db.Users.Where(user => user.UserId == userId).CountAsync();
            return userCount > 0;
        }
        public async Task<bool> CheckEmail(string email)
        {
            var userCount = await db.Users.Where(user => user.Email == email).CountAsync();
            return userCount > 0;
        }

        public async Task<bool> UpdateUserProfile(UserProfileUpdate updates, User user)
        {
            if (!updates.Username.IsNullOrEmpty())
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                user.Username = updates.Username;
#pragma warning restore CS8601 // Possible null reference assignment.
            }
            if (!updates.ProfilePicture.IsNullOrEmpty())
            {
                user.ProfilePicture = updates.ProfilePicture;
            }
            if (!updates.Biography.IsNullOrEmpty())
            {
                user.Biography = updates.Biography;
            }
            var res = await db.SaveChangesAsync();
            return res == 1;
        }

        public async Task<Role?> GetRole(string roleName) => await db.Roles.Where(role => role.RoleName == roleName).FirstOrDefaultAsync();

        public async Task<bool> CreateBugReport(int userId, string message)
        {
            db.BugReports.Add(new BugReport()
            {
                UserId = userId,
                Message = message,
                ReportedDatetime = DateTime.Now,
                Status = "open"
            });

            var res = await db.SaveChangesAsync();

            return res == 1;
        }



    }
}
