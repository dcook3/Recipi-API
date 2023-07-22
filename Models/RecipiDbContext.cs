using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Recipi_API.Models;

public partial class RecipiDbContext : DbContext
{
    public RecipiDbContext()
    {
    }

    public RecipiDbContext(DbContextOptions<RecipiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BugReport> BugReports { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostComment> PostComments { get; set; }

    public virtual DbSet<PostInteraction> PostInteractions { get; set; }

    public virtual DbSet<PostMedium> PostMedia { get; set; }

    public virtual DbSet<PostReport> PostReports { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeCookbook> RecipeCookbooks { get; set; }

    public virtual DbSet<RecipeStep> RecipeSteps { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StepIngredient> StepIngredients { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRelationship> UserRelationships { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=recipi-db-mssql.cmnzbcgsvlqu.us-east-2.rds.amazonaws.com,1433;Database=recipi-db;User ID=JDLRecipi;Password=Capstone_2023;Encrypt=True;Connection Timeout=30;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BugReport>(entity =>
        {
            entity.ToTable("BugReport");

            entity.Property(e => e.BugReportId).HasColumnName("bug_report_id");
            entity.Property(e => e.Message)
                .HasMaxLength(250)
                .HasColumnName("message");
            entity.Property(e => e.ReportedDatetime)
                .HasPrecision(3)
                .HasColumnName("reported_datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.BugReports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BugReport_Users");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversation");

            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.UserId1).HasColumnName("user_id1");
            entity.Property(e => e.UserId2).HasColumnName("user_id2");

            entity.HasOne(d => d.UserId1Navigation).WithMany(p => p.ConversationUserId1Navigations)
                .HasForeignKey(d => d.UserId1)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversation_Users1");

            entity.HasOne(d => d.UserId2Navigation).WithMany(p => p.ConversationUserId2Navigations)
                .HasForeignKey(d => d.UserId2)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversation_Users2");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.IngredientDescription)
                .HasMaxLength(200)
                .HasColumnName("ingredient_description");
            entity.Property(e => e.IngredientIcon)
                .HasMaxLength(2043)
                .HasColumnName("ingredient_icon");
            entity.Property(e => e.IngredientTitle)
                .HasMaxLength(50)
                .HasColumnName("ingredient_title");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Ingredients)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingredients_Users");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.MessageContents)
                .HasMaxLength(250)
                .HasColumnName("message_contents");
            entity.Property(e => e.SendingUserId).HasColumnName("sending_user_id");
            entity.Property(e => e.SentDatetime)
                .HasPrecision(3)
                .HasColumnName("sent_datetime");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Conversation");

            entity.HasOne(d => d.SendingUser).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SendingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Users");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.PostDescription)
                .HasMaxLength(200)
                .HasColumnName("post_description");
            entity.Property(e => e.PostMedia)
                .HasMaxLength(2043)
                .HasColumnName("post_media");
            entity.Property(e => e.PostTitle)
                .HasMaxLength(50)
                .HasColumnName("post_title");
            entity.Property(e => e.PostedDatetime).HasColumnName("posted_datetime");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(2043)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<PostComment>(entity =>
        {
            entity.HasKey(e => e.CommentId);

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(200)
                .HasColumnName("comment");
            entity.Property(e => e.CommentDatetime)
                .HasPrecision(3)
                .HasColumnName("comment_datetime");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostComments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_PostComments_Posts");

            entity.HasOne(d => d.User).WithMany(p => p.PostComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostComments_Users");
        });

        modelBuilder.Entity<PostInteraction>(entity =>
        {
            entity.HasKey(e => e.InteractionId).HasName("PK_PostLikes");

            entity.ToTable("PostInteraction");

            entity.Property(e => e.InteractionId).HasColumnName("interaction_id");
            entity.Property(e => e.InteractionDatetime).HasColumnName("interaction_datetime");
            entity.Property(e => e.Liked)
                .HasDefaultValueSql("((0))")
                .HasColumnName("liked");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostInteractions)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_PostLikes_Posts");

            entity.HasOne(d => d.User).WithMany(p => p.PostInteractions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostInteraction_Users");
        });

        modelBuilder.Entity<PostMedium>(entity =>
        {
            entity.HasKey(e => e.PostMediaId);

            entity.Property(e => e.PostMediaId).HasColumnName("post_media_id");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(2043)
                .HasColumnName("media_url");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.StepId).HasColumnName("step_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostMediaNavigation)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_PostMedia_Posts");

            entity.HasOne(d => d.Step).WithMany(p => p.PostMedia)
                .HasForeignKey(d => d.StepId)
                .HasConstraintName("FK_PostMedia_RecipeSteps");
        });

        modelBuilder.Entity<PostReport>(entity =>
        {
            entity.Property(e => e.PostReportId).HasColumnName("post_report_id");
            entity.Property(e => e.Message)
                .HasMaxLength(250)
                .HasColumnName("message");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.ReportedDatetime)
                .HasPrecision(3)
                .HasColumnName("reported_datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostReports)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_PostReports_Posts");

            entity.HasOne(d => d.User).WithMany(p => p.PostReports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostReports_Users");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.CreatedDatetime)
                .HasColumnType("datetime")
                .HasColumnName("created_datetime");
            entity.Property(e => e.RecipeDescription)
                .HasMaxLength(200)
                .HasColumnName("recipe_description");
            entity.Property(e => e.RecipeTitle)
                .HasMaxLength(50)
                .HasColumnName("recipe_title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recipes_Users");
        });

        modelBuilder.Entity<RecipeCookbook>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RecipeId });

            entity.ToTable("RecipeCookbook");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.RecipeOrder).HasColumnName("recipe_order");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeCookbooks)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK_RecipeCookbook_Recipes");

            entity.HasOne(d => d.User).WithMany(p => p.RecipeCookbooks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RecipeCookbook_Users");
        });

        modelBuilder.Entity<RecipeStep>(entity =>
        {
            entity.HasKey(e => e.StepId);

            entity.Property(e => e.StepId).HasColumnName("step_id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.StepDescription)
                .HasMaxLength(200)
                .HasColumnName("step_description");
            entity.Property(e => e.StepOrder).HasColumnName("step_order");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeSteps)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK_RecipeSteps_Recipes");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<StepIngredient>(entity =>
        {
            entity.HasKey(e => new { e.StepId, e.IngredientId });

            entity.Property(e => e.StepId).HasColumnName("step_id");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.IngredientMeasurementUnit)
                .HasMaxLength(10)
                .HasColumnName("ingredient_measurement_unit");
            entity.Property(e => e.IngredientMeasurementValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("ingredient_measurement_value");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.StepIngredients)
                .HasForeignKey(d => d.IngredientId)
                .HasConstraintName("FK_StepIngredients_Ingredients");

            entity.HasOne(d => d.Step).WithMany(p => p.StepIngredients)
                .HasForeignKey(d => d.StepId)
                .HasConstraintName("FK_StepIngredients_RecipeSteps");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Biography)
                .HasMaxLength(200)
                .HasColumnName("biography");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.ProfilePicture)
                .HasMaxLength(2048)
                .HasColumnName("profile_picture");
            entity.Property(e => e.RegisteredDatetime)
                .HasPrecision(3)
                .HasColumnName("registered_datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(30)
                .HasColumnName("username");
            entity.Property(e => e.Verified).HasColumnName("verified");
        });

        modelBuilder.Entity<UserRelationship>(entity =>
        {
            entity.HasKey(e => new { e.InitiatingUserId, e.ReceivingUserId });

            entity.ToTable("UserRelationship");

            entity.Property(e => e.InitiatingUserId).HasColumnName("initiating_user_id");
            entity.Property(e => e.ReceivingUserId).HasColumnName("receiving_user_id");
            entity.Property(e => e.InitiatedDatetime)
                .HasPrecision(3)
                .HasColumnName("initiated_datetime");
            entity.Property(e => e.Relationship)
                .HasMaxLength(20)
                .HasColumnName("relationship");

            entity.HasOne(d => d.InitiatingUser).WithMany(p => p.UserRelationshipInitiatingUsers)
                .HasForeignKey(d => d.InitiatingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRelationship_Users_Initiating");

            entity.HasOne(d => d.ReceivingUser).WithMany(p => p.UserRelationshipReceivingUsers)
                .HasForeignKey(d => d.ReceivingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRelationship_Users_Receiving");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.UserId });

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ExpirationDays).HasColumnName("expiration_days");
            entity.Property(e => e.GrantedDatetime)
                .HasPrecision(3)
                .HasColumnName("granted_datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserRoles_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
