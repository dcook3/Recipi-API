using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public class PostCommentData
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Comment { get; set; } = null!;
}
