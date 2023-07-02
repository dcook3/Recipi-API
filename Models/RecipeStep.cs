using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class RecipeStep
{
    public int StepId { get; set; }

    public string StepDescription { get; set; } = null!;

    public int RecipeId { get; set; }

    public short StepOrder { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;
}
