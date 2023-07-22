using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class RecipeStep
{
    public int StepId { get; set; }

    public string StepDescription { get; set; } = null!;

    public int RecipeId { get; set; }

    public short StepOrder { get; set; }

    public virtual ICollection<PostMedium> PostMedia { get; set; } = new List<PostMedium>();

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual ICollection<StepIngredient> StepIngredients { get; set; } = new List<StepIngredient>();
}
