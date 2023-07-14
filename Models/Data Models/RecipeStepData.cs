namespace Recipi_API.Models.Data_Models
{
    public class RecipeStepData
    {
        public int StepId { get; set; }

        public string StepDescription { get; set; } = null!;

        public int RecipeId { get; set; }

        public short StepOrder { get; set; }

        public int ingredientMeasuremnetValue { get; set; }
        public string ingredientMeasurementLabel { get; set; }

        public ICollection<PostMedium> PostMedia { get; set; } = new List<PostMedium>();

        public virtual ICollection<Ingredient> StepIngredients { get; set; } = new List<Ingredient>();
    }
}
