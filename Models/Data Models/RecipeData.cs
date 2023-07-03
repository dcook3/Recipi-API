using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class RecipeData
{
    public string RecipeTitle { get; set; } = null!;

    public string? RecipeDescription { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedDatetime { get; set; }
}
