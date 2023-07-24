namespace Recipi_API.Models.Data_Models
{
    public class RecipeStepData
    {
        public string StepDescription { get; set; } = null!;

        public short StepOrder { get; set; }

        public virtual List<StepIngredientData> StepIngredients { get; set; } = new List<StepIngredientData>()!;
    }
}
