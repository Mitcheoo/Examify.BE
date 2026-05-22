// Examify.Core/Entities/FullTestSession.cs
namespace Examify.Core.Entities;

public class FullTestSession : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public short? TotalScore { get; set; }
    public Guid? ReadingSubmissionId { get; set; }
    public Guid? ListeningSubmissionId { get; set; }
    public Guid? WritingSubmissionId { get; set; }
    public Guid? SpeakingSubmissionId { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Submission? ReadingSubmission { get; set; }
    public virtual Submission? ListeningSubmission { get; set; }
    public virtual Submission? WritingSubmission { get; set; }
    public virtual Submission? SpeakingSubmission { get; set; }
}