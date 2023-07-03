using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class Ingredient
{
    public int IngredientId { get; set; }

    public string IngredientTitle { get; set; } = null!;

    public string? IngredientDescription { get; set; }

    public int CreatedByUserId { get; set; }

    public string? IngredientIcon { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<StepIngredient> StepIngredients { get; set; } = new List<StepIngredient>();
}
