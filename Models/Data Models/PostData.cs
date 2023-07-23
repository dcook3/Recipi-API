namespace Recipi_API.Models.Data_Models
{
    public class PostData
    {
        public string PostTitle { get; set; } = null!;

        public string PostDescription { get; set; } = null!;

        public int? RecipeId { get; set; }

        public string? PostMedia { get; set; }

        public string ThumbnailUrl { get; set; } = null!;
    }
}
