// Examify.Core/Entities/SubmissionDetail.cs
namespace Examify.Core.Entities;

public class SubmissionDetail : BaseEntity
{
    public Guid SubmissionId { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public string? UserAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public short PointEarned { get; set; }
    public double? AiScore { get; set; }
    public string? AiFeedback { get; set; }

    // Navigation
    public virtual Submission Submission { get; set; } = null!;
}