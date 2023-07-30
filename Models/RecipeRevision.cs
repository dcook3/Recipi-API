using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class RecipeRevision
{
    public int RevisionId { get; set; }

    public int OldRecipeId { get; set; }

    public int NewRecipeId { get; set; }

    public string Revision { get; set; } = null!;

    public virtual Recipe NewRecipe { get; set; } = null!;

    public virtual Recipe OldRecipe { get; set; } = null!;
}
