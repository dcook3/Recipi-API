using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class RecipeCookbook
{
    public int UserId { get; set; }

    public int RecipeId { get; set; }

    public short RecipeOrder { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
