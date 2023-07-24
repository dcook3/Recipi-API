using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class StepIngredient
{
    public int StepIngredientId { get; set; }

    public int StepId { get; set; }

    public int IngredientId { get; set; }

    public string IngredientMeasurementUnit { get; set; } = null!;

    public decimal IngredientMeasurementValue { get; set; }

    public virtual Ingredient Ingredient { get; set; } = null!;

    public virtual RecipeStep Step { get; set; } = null!;
}
