// Examify.Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Examify.Core.Entities;

namespace Examify.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<ReadingQuestion> ReadingQuestions { get; set; }
    public DbSet<ListeningQuestion> ListeningQuestions { get; set; }
    public DbSet<WritingQuestion> WritingQuestions { get; set; }
    public DbSet<SpeakingQuestion> SpeakingQuestions { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<SubmissionDetail> SubmissionDetails { get; set; }
    public DbSet<FullTestSession> FullTestSessions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Leaderboard> Leaderboards { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Exercise configuration
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("Exercises");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Skill).IsRequired();
            entity.Property(e => e.TotalParts).HasDefaultValue(3);
            entity.Property(e => e.TotalQuestions).HasDefaultValue(0);
            entity.Property(e => e.TimeLimitSeconds).HasDefaultValue(3600);
        });

        // Cấu hình cho các entity khác tương tự...
    }
}