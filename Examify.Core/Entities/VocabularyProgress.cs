// Examify.Core/Entities/VocabularyProgress.cs
namespace Examify.Core.Entities;

public class VocabularyProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid VocabularyWordId { get; set; }

    public bool IsMastered { get; set; } = false;
    public int ReviewCount { get; set; } = 0;

    // Spaced Repetition fields
    public DateTime? LastReviewedAt { get; set; }
    public DateTime? NextReviewAt { get; set; } // Ngày cần ôn tập tiếp theo
    public int StreakCount { get; set; } = 0; // Số ngày liên tục học từ này

    // Stats
    public int CorrectCount { get; set; } = 0;
    public int IncorrectCount { get; set; } = 0;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual VocabularyWord VocabularyWord { get; set; } = null!;
}