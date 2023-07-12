using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;

namespace Recipi_API.Services
{
    public class UserService
    {
        private static RecipiDbContext db = new RecipiDbContext();


        public async Task<User?> GetUser(int id) => await db.Users.Where(user => user.UserId == id).FirstOrDefaultAsync();
        public async Task<User?> GetUser(string username) => await db.Users.Where(user => user.Username == username).FirstOrDefaultAsync();

        public async Task<bool> CreateUser(UserRegistration registration) {

            User user = new User()
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

            UserRole role = new UserRole()
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

        public async Task<List<string>> GetRelationships(int userId1, int userId2) =>
            await db.UserRelationships.Where(rel => (rel.InitiatingUserId == userId1 || rel.InitiatingUserId == userId2) ||
                                                    (rel.ReceivingUserId == userId1 || rel.ReceivingUserId == userId2))
                                                    .Select(rel => rel.Relationship)
                                                    .ToListAsync();

        public async Task<bool> AddFriend(int initiatingUserId, int recievingUserId)
        {
            db.UserRelationships.Add(new UserRelationship() { 
                InitiatingUserId = initiatingUserId,
                ReceivingUserId = recievingUserId,
                Relationship = "friend",
                InitiatedDatetime= DateTime.Now
            });
            var res = await db.SaveChangesAsync();
            return res == 1;
        }
        public async Task<bool> RemoveFriend(int initiatingUserId, int recievingUserId)
        {
            var rel = await db.UserRelationships.Where(rel => (rel.InitiatingUserId == initiatingUserId || rel.ReceivingUserId == recievingUserId) && rel.Relationship == "friend").FirstOrDefaultAsync();

            if (rel == null) return false;

            db.UserRelationships.Remove(rel);               
            var res = await db.SaveChangesAsync();
            return res == 1;
        }
        public async Task<List<User>> GetFriends(int userId) =>
            await db.UserRelationships.Where(rel => (rel.InitiatingUserId == userId || rel.ReceivingUserId == userId)
                                                    && rel.Relationship == "friend")
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
        public async Task<bool> UnfollowUser(int initiatingUserId, int recievingUserId)
        {
            var rel = await db.UserRelationships.Where(rel => (rel.InitiatingUserId == initiatingUserId || rel.ReceivingUserId == recievingUserId) && rel.Relationship == "follow").FirstOrDefaultAsync();

            if (rel == null) return false;

            db.UserRelationships.Remove(rel);
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
            if(blocks.Count() == 0)
            {
                return BlockStatus.None;
            }
            //both users are blocking
            if(blocks.Count() > 1)
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

            if(rels.Count > 0)
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
        public async Task<bool> UnblockUser(int initiatingUserId, int recievingUserId)
        {
            var rel = await db.UserRelationships.Where(rel => rel.InitiatingUserId == initiatingUserId && rel.Relationship == "block").FirstOrDefaultAsync();

            if (rel == null)
            {
                return false;
            }

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

        public async Task<Role?> GetRole(string roleName) => await db.Roles.Where(role => role.RoleName == roleName).FirstOrDefaultAsync();

    }
}
