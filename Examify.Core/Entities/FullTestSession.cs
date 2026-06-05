// Examify.Core/Entities/FullTestSession.cs
namespace Examify.Core.Entities;

public class FullTestSession : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public short? TotalScore { get; set; }

    // ✅ THÊM CÁC TRƯỜNG NÀY
    public int Status { get; set; } = 0; // 0=InProgress, 1=Completed, 2=Expired
    public int CurrentPart { get; set; } = 1; // 1=Reading,2=Listening,3=Writing,4=Speaking

    // Thời gian làm từng phần (giây)
    public int ReadingTimeSpent { get; set; }
    public int ListeningTimeSpent { get; set; }
    public int WritingTimeSpent { get; set; }
    public int SpeakingTimeSpent { get; set; }

    // ID của các bài thi con
    public Guid? ReadingExerciseId { get; set; }
    public Guid? ListeningExerciseId { get; set; }
    public Guid? WritingExerciseId { get; set; }
    public Guid? SpeakingExerciseId { get; set; }

    // ID của các bài nộp
    public Guid? ReadingSubmissionId { get; set; }
    public Guid? ListeningSubmissionId { get; set; }
    public Guid? WritingSubmissionId { get; set; }
    public Guid? SpeakingSubmissionId { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Exercise? ReadingExercise { get; set; }
    public virtual Exercise? ListeningExercise { get; set; }
    public virtual Exercise? WritingExercise { get; set; }
    public virtual Exercise? SpeakingExercise { get; set; }
    public virtual Submission? ReadingSubmission { get; set; }
    public virtual Submission? ListeningSubmission { get; set; }
    public virtual Submission? WritingSubmission { get; set; }
    public virtual Submission? SpeakingSubmission { get; set; }
}