// Examify.Infrastructure/Data/ApplicationDbContext.cs
using Examify.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Examify.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ReadingQuestion> ReadingQuestions { get; set; }
    public DbSet<ListeningQuestion> ListeningQuestions { get; set; }
    public DbSet<WritingQuestion> WritingQuestions { get; set; }
    public DbSet<SpeakingQuestion> SpeakingQuestions { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<SubmissionDetail> SubmissionDetails { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Leaderboard> Leaderboards { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<FullTestSession> FullTestSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình Exercise
        builder.Entity<Exercise>(entity =>
        {
            entity.ToTable("Exercises");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);

            // ✅ THÊM CẤU HÌNH CHO CÁC PROPERTY LIÊN KẾT FULL TEST
            entity.HasOne(e => e.ReadingExercise)
                  .WithMany()
                  .HasForeignKey(e => e.ReadingExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ListeningExercise)
                  .WithMany()
                  .HasForeignKey(e => e.ListeningExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WritingExercise)
                  .WithMany()
                  .HasForeignKey(e => e.WritingExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SpeakingExercise)
                  .WithMany()
                  .HasForeignKey(e => e.SpeakingExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Cấu hình Part
        builder.Entity<Part>(entity =>
        {
            entity.ToTable("Parts");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Exercise)
                  .WithMany(e => e.Parts)
                  .HasForeignKey(e => e.ExerciseId);
        });

        // ✅ CẤU HÌNH CHO FULL TEST SESSION
        builder.Entity<FullTestSession>(entity =>
        {
            entity.ToTable("FullTestSessions");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.ReadingExercise)
                  .WithMany()
                  .HasForeignKey(e => e.ReadingExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ListeningExercise)
                  .WithMany()
                  .HasForeignKey(e => e.ListeningExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WritingExercise)
                  .WithMany()
                  .HasForeignKey(e => e.WritingExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SpeakingExercise)
                  .WithMany()
                  .HasForeignKey(e => e.SpeakingExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReadingSubmission)
                  .WithMany()
                  .HasForeignKey(e => e.ReadingSubmissionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ListeningSubmission)
                  .WithMany()
                  .HasForeignKey(e => e.ListeningSubmissionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WritingSubmission)
                  .WithMany()
                  .HasForeignKey(e => e.WritingSubmissionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SpeakingSubmission)
                  .WithMany()
                  .HasForeignKey(e => e.SpeakingSubmissionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<SessionAnswer>(entity =>
        {
            entity.ToTable("SessionAnswers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserAnswer).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AudioUrl).HasMaxLength(500);
            entity.Property(e => e.Transcript).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IsSubmitted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");  // ✅ DÙNG UpdatedAt TỪ BaseEntity

            entity.HasOne(e => e.Session)
                  .WithMany()
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SessionId, e.QuestionId })
                  .IsUnique()
                  .HasDatabaseName("IX_SessionAnswers_SessionId_QuestionId");

            entity.HasIndex(e => e.SessionId)
                  .HasDatabaseName("IX_SessionAnswers_SessionId");
        });
    }
}