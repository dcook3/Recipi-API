using System;
using System.ComponentModel.DataAnnotations;

namespace Recipi_API.Models
{
    public partial class SignedUrlRequest
    {
        public string Content { get; set; } = null!;

        public double TimeToLiveInHours = 1;
    }
}
