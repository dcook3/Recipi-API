using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class PostLike
{
    public int LikeId { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
