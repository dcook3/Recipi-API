using RecipiDBlib.Models;

namespace Recipi_API.Services
{
    public class UserService
    {
        private static RecipiDbContext db = new RecipiDbContext();

        public User? GetUser(int id) => db.Users.Where(user => user.UserId == id).FirstOrDefault();
        
    }
}
