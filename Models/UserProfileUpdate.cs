using System.ComponentModel.DataAnnotations;

namespace Recipi_API.Models
{
    public class UserProfileUpdate
    {
        
        [MaxLength(30)]
        public string? Username { get; set; }

        [MaxLength(2048)]
        public string? ProfilePicture { get; set; }

        [MaxLength(200)]
        public string? Biography { get; set; }
    }
}
