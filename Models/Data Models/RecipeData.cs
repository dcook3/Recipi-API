using Recipi_API.Models.Data_Models;
using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class RecipeData
{
    public string RecipeTitle { get; set; } = null!;

    public string? RecipeDescription { get; set; }

    public List<RecipeStepData> RecipeSteps { get; set; } = new()!;
}
