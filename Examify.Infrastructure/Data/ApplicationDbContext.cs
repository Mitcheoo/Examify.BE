// Examify.Infrastructure/Data/ApplicationDbContext.cs
using Examify.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Examify.Infrastructure.Data;

// ✅ KẾ THỪA ĐÚNG TỪ IdentityDbContext
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets cho các entity khác
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ReadingQuestion> ReadingQuestions { get; set; }
    public DbSet<ListeningQuestion> ListeningQuestions { get; set; }
    public DbSet<WritingQuestion> WritingQuestions { get; set; }
    public DbSet<SpeakingQuestion> SpeakingQuestions { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<SubmissionDetail> SubmissionDetails { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Leaderboard> Leaderboards { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<FullTestSession> FullTestSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);  // ✅ PHẢI GỌI base

        // Các cấu hình khác...
        builder.Entity<Exercise>(entity =>
        {
            entity.ToTable("Exercises");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
        });
    }
}