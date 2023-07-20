using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class PostComment
{
    public int CommentId { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Comment { get; set; } = null!;

    public DateTime CommentDatetime { get; set; }

    public virtual Post Post { get; set; } = null!;
}
