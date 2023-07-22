using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class PostInteraction
{
    public int InteractionId { get; set; }

    public int PostId { get; set; }

    public bool? Liked { get; set; }

    public DateTime? InteractionDatetime { get; set; }

    public int UserId { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
