// Examify.Core/Entities/Submission.cs
using System.Transactions;

namespace Examify.Core.Entities;

public class Submission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ExerciseId { get; set; }
    public int SkillType { get; set; }
    public short TotalScore { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public int TimeSpentSeconds { get; set; }
    public string? ResultJson { get; set; }
    public string? EssayText { get; set; }
    public string? AudioUrl { get; set; }
    public string? Transcript { get; set; }
    public string? AiFeedback { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsGraded { get; set; } = true;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Exercise Exercise { get; set; } = null!;
    public virtual ICollection<SubmissionDetail> Details { get; set; } = new List<SubmissionDetail>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}