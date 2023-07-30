using System.ComponentModel.DataAnnotations;

namespace Recipi_API.Models.Data_Models
{
    public class RecipeStepData
    {
        [Required]
        public string StepDescription { get; set; } = null!;

        [Required]
        public short StepOrder { get; set; }

        [ValidateList<StepIngredientData>]
        public virtual ICollection<StepIngredientData> StepIngredients { get; set; } = new List<StepIngredientData>()!;
    }
}
