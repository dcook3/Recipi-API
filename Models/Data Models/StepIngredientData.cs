using System.ComponentModel.DataAnnotations;

namespace Recipi_API.Models.Data_Models
{
    public class StepIngredientData
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please Enter Valid Ingredient Id")]
        public int IngredientId { get; set; }

        [Required]
        public string IngredientMeasurementUnit { get; set; } = null!;
        [Required]
        public decimal IngredientMeasurementValue { get; set; }
    }
}
