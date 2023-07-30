using Recipi_API.Models.Data_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Recipi_API.Models;

public partial class RecipeData
{
    [Required]
    public string RecipeTitle { get; set; } = null!;

    public string? RecipeDescription { get; set; }

    [ValidateList<RecipeStepData>]
    public List<RecipeStepData> RecipeSteps { get; set; } = new()!;
}
