using Microsoft.EntityFrameworkCore;
using BaseLibrary.Entities.Base;
using BaseLibrary.Entities.Auth;
using BaseLibrary.Entities.Groups;
using BaseLibrary.Entities.Posts;
using BaseLibrary.Entities.Quiz;
using BaseLibrary.Entities.Survey;

namespace ServerLibrary.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Domain> Domains { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<PostType> PostTypes { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostSection> PostSections { get; set; }
    public DbSet<PostHashtag> PostHashtags { get; set; }
    public DbSet<PostMedia> PostMedia { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentVote> CommentVotes { get; set; }
    public DbSet<PostRating> PostRatings { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizOption> QuizOptions { get; set; }
    public DbSet<QuizResponse> QuizResponses { get; set; }
    public DbSet<SurveyOption> SurveyOptions { get; set; }
    public DbSet<SurveyVote> SurveyVotes { get; set; }
    public DbSet<ReportReason> ReportReasons { get; set; }
    public DbSet<PostReport> PostReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration minimale
        modelBuilder.Entity<GroupMember>()
            .HasIndex(gm => new { gm.GroupId, gm.UserId })
            .IsUnique();

        modelBuilder.Entity<CommentVote>()
            .HasIndex(cv => new { cv.CommentId, cv.UserId })
            .IsUnique();

        modelBuilder.Entity<SurveyVote>()
            .HasIndex(sv => new { sv.PostId, sv.UserId })
            .IsUnique();
    }
}
