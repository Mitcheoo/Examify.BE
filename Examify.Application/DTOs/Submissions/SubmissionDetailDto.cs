// Examify.Application/DTOs/Submissions/SubmissionDetailDto.cs
namespace Examify.Application.DTOs.Submissions;

public class SubmissionDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int Skill { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public double TotalScore { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public DateTime SubmittedAt { get; set; }

    // ✅ THÊM 3 TRƯỜNG NÀY CHO SPEAKING
    public string? AudioUrl { get; set; }
    public string? Transcript { get; set; }
    public string? AnswerJson { get; set; }

    public List<SubmissionAnswerDetailDto> Details { get; set; } = new();
    public object? AiFeedback { get; set; }
}

public class SubmissionAnswerDetailDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public string? UserAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public double? AiScore { get; set; }
    public string? AiFeedback { get; set; }
    public string? Explanation { get; set; }
}