// 📁 Examify.Infrastructure/Data/ApplicationDbContext.cs

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
    public DbSet<PurchasedExercise> PurchasedExercises { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Leaderboard> Leaderboards { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<FullTestSession> FullTestSessions { get; set; }
    public DbSet<SessionAnswer> SessionAnswers { get; set; } // ✅ THÊM DbSet NÀY
    public DbSet<VocabularyWord> VocabularyWords { get; set; }
    public DbSet<VocabularyProgress> VocabularyProgress { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình Exercise
        builder.Entity<Exercise>(entity =>
        {
            entity.ToTable("Exercises");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);

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
            entity.Property(e => e.IsFree)
                .HasDefaultValue(true);

            entity.Property(e => e.Price)
                  .HasPrecision(18, 2)
                  .HasDefaultValue(0);
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
        builder.Entity<VocabularyWord>(entity =>
        {
            entity.ToTable("VocabularyWords");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Word)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Meaning)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.Example)
                  .IsRequired()
                  .HasMaxLength(1000);

            entity.Property(e => e.Pronunciation)
                  .HasMaxLength(50);

            entity.Property(e => e.PartOfSpeech)
                  .HasMaxLength(50);

            entity.Property(e => e.Level)
                  .HasMaxLength(10)
                  .HasDefaultValue("B1");

            entity.Property(e => e.Topic)
                  .HasMaxLength(50);

            entity.Property(e => e.AudioUrl)
                  .HasMaxLength(500);

            entity.Property(e => e.ImageUrl)
                  .HasMaxLength(500);

            entity.Property(e => e.VietnameseExample)
                  .HasMaxLength(500);

            entity.HasIndex(e => e.Word).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.Topic);
            entity.HasIndex(e => e.CreatedAt);
        });

        builder.Entity<VocabularyProgress>(entity =>
        {
            entity.ToTable("VocabularyProgress");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IsMastered)
                  .HasDefaultValue(false);

            entity.Property(e => e.ReviewCount)
                  .HasDefaultValue(0);

            entity.Property(e => e.StreakCount)
                  .HasDefaultValue(0);

            entity.Property(e => e.CorrectCount)
                  .HasDefaultValue(0);

            entity.Property(e => e.IncorrectCount)
                  .HasDefaultValue(0);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.VocabularyWord)
                  .WithMany(w => w.Progress)
                  .HasForeignKey(e => e.VocabularyWordId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.VocabularyWordId })
                  .IsUnique()
                  .HasFilter("[IsDeleted] = 0");

            entity.HasIndex(e => e.IsMastered);
            entity.HasIndex(e => e.NextReviewAt);
            entity.HasIndex(e => e.LastReviewedAt);
            entity.HasIndex(e => e.UserId);
        });
    

    // Cấu hình Full Test Session
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

        // ============================================================
        // CẤU HÌNH SESSION ANSWER - CHỈ SỬA ĐÚNG PHẦN INDEX
        // ============================================================
        builder.Entity<SessionAnswer>(entity =>
        {
            entity.ToTable("SessionAnswers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserAnswer).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AudioUrl).HasMaxLength(500);
            entity.Property(e => e.Transcript).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IsSubmitted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Session)
                  .WithMany()
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

            // ✅ CHỈ SỬA ĐÚNG DÒNG NÀY - THÊM HASFILTER
            entity.HasIndex(e => new { e.SessionId, e.QuestionId })
                  .IsUnique()
                  .HasDatabaseName("IX_SessionAnswers_SessionId_QuestionId")
                  .HasFilter("[IsDeleted] = 0");  // ✅ THÊM DÒNG NÀY

            entity.HasIndex(e => e.SessionId)
                  .HasDatabaseName("IX_SessionAnswers_SessionId");
        });
        builder.Entity<Wallet>(entity =>
        {
            entity.ToTable("Wallets");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Balance)
                  .HasPrecision(18, 2)
                  .HasDefaultValue(0);

            entity.Property(e => e.TotalDeposited)
                  .HasPrecision(18, 2)
                  .HasDefaultValue(0);

            entity.Property(e => e.TotalSpent)
                  .HasPrecision(18, 2)
                  .HasDefaultValue(0);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Wallet)
                  .HasForeignKey<Wallet>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // ============================================================
        // ✅ CẤU HÌNH TRANSACTION (THÊM MỚI)
        // ============================================================
        builder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Amount)
                  .HasPrecision(18, 2)
                  .IsRequired();

            entity.Property(e => e.BalanceBefore)
                  .HasPrecision(18, 2)
                  .IsRequired();

            entity.Property(e => e.BalanceAfter)
                  .HasPrecision(18, 2)
                  .IsRequired();

            entity.Property(e => e.Type)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Status)
                  .HasMaxLength(20)
                  .IsRequired()
                  .HasDefaultValue("Pending");

            entity.Property(e => e.PaymentMethod)
                  .HasMaxLength(20);

            entity.Property(e => e.PayPalOrderId)
                  .HasMaxLength(100);

            entity.Property(e => e.PayPalCaptureId)
                  .HasMaxLength(100);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Wallet)
                  .WithMany(w => w.Transactions)
                  .HasForeignKey(e => e.WalletId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Exercise)
                  .WithMany()
                  .HasForeignKey(e => e.ExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.PayPalOrderId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ============================================================
        // ✅ CẤU HÌNH PURCHASED EXERCISE (THÊM MỚI)
        // ============================================================
        builder.Entity<PurchasedExercise>(entity =>
        {
            entity.ToTable("PurchasedExercises");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PaidAmount)
                  .HasPrecision(18, 2)
                  .IsRequired();

            entity.Property(e => e.PurchasedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Exercise)
                  .WithMany()
                  .HasForeignKey(e => e.ExerciseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ExerciseId })
                  .IsUnique()
                  .HasFilter("[IsDeleted] = 0");
        });
    }
}