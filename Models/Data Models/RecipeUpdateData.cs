using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Recipi_API.Models.Data_Models
{
    public class RecipeUpdateData
    {
        
        public string? RecipeTitle { get; set; }

        public string? RecipeDescription { get; set; }

        [ValidateList<RecipeStepData>]
        public List<RecipeStepData> RecipeSteps { get; set; } = new()!;
    }
}
