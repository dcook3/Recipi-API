using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class Recipe
{
    public int RecipeId { get; set; }

    public string RecipeTitle { get; set; } = null!;

    public string? RecipeDescription { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public virtual ICollection<RecipeCookbook> RecipeCookbooks { get; set; } = new List<RecipeCookbook>();

    public virtual ICollection<RecipeStep> RecipeSteps { get; set; } = new List<RecipeStep>();

    public virtual User User { get; set; } = null!;
}
