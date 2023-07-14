using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class RecipeData
{
    public int RecipeId { get; set; }
    public string RecipeTitle { get; set; } = null!;

    public string? RecipeDescription { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public List<RecipeStep> RecipeSteps { get; set; } = new List<RecipeStep>();
}
