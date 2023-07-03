using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class Post
{
    public int PostId { get; set; }

    public string PostTitle { get; set; } = null!;

    public string PostDescription { get; set; } = null!;

    public int UserId { get; set; }

    public int? RecipeId { get; set; }

    public string? PostMedia { get; set; }

    public DateTime PostedDatetime { get; set; }

    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();

    public virtual ICollection<PostMedium> PostMediaNavigation { get; set; } = new List<PostMedium>();

    public virtual ICollection<PostReport> PostReports { get; set; } = new List<PostReport>();

    public virtual Recipe? Recipe { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
