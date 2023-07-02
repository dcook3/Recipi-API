using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? ProfilePicture { get; set; }

    public string? Biography { get; set; }

    public byte Verified { get; set; }

    public DateTime RegisteredDatetime { get; set; }

    public virtual ICollection<BugReport> BugReports { get; set; } = new List<BugReport>();

    public virtual ICollection<Conversation> ConversationUserId1Navigations { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationUserId2Navigations { get; set; } = new List<Conversation>();

    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual ICollection<PostReport> PostReports { get; set; } = new List<PostReport>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<RecipeCookbook> RecipeCookbooks { get; set; } = new List<RecipeCookbook>();

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual ICollection<UserRelationship> UserRelationshipInitiatingUsers { get; set; } = new List<UserRelationship>();

    public virtual ICollection<UserRelationship> UserRelationshipReceivingUsers { get; set; } = new List<UserRelationship>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
