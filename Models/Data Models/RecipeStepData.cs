namespace Recipi_API.Models.Data_Models
{
    public class RecipeStepData
    {
        public int StepId { get; set; }

        public string StepDescription { get; set; } = null!;

        public int RecipeId { get; set; }

        public short StepOrder { get; set; }

        public virtual ICollection<StepIngredient> StepIngredients { get; set; } = new List<StepIngredient>();
    }
}
