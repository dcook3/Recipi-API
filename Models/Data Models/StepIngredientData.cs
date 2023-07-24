namespace Recipi_API.Models.Data_Models
{
    public class StepIngredientData
    {
        public int IngredientId { get; set; }

        public string IngredientMeasurementUnit { get; set; } = null!;

        public decimal IngredientMeasurementValue { get; set; }
    }
}
