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
            


        public async Task<bool> CheckUsername(string username)
        {
            var userCount = await db.Users.Where(user => user.Username == username).CountAsync();       
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
