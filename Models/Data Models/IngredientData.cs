namespace Recipi_API.Models.Data_Models
{
    public class IngredientData
    {
        public string IngredientTitle { get; set; } = null!;

        public string? IngredientDescription { get; set; }

        public int CreatedByUserId { get; set; }

        public string? IngredientIcon { get; set; }
    }
}
