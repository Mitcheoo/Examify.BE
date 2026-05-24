// Examify.Application/DTOs/Submissions/SubmissionResultDto.cs
namespace Examify.Application.DTOs.Submissions;

public class SubmissionResultDto
{
    public Guid SubmissionId { get; set; }
    public Guid UserId { get; set; }           // ✅ THÊM DÒNG NÀY
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public short TotalScore { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public int TimeSpentSeconds { get; set; }
    public DateTime SubmittedAt { get; set; }
    public List<SubmissionDetailDto> Details { get; set; } = new();
}

public class SubmissionDetailDto
{
    public Guid QuestionId { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}