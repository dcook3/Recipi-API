using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class PostMedium
{
    public int PostMediaId { get; set; }

    public int PostId { get; set; }

    public int? StepId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;

    public virtual RecipeStep? Step { get; set; }
}
