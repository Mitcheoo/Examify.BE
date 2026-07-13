// Examify.Application/DTOs/Admin/AdminSubmissionDto.cs
namespace Examify.Application.DTOs.Admin;

public class AdminSubmissionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int SkillType { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public double Score { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public int TimeSpentSeconds { get; set; }
    public string TimeSpentFormatted { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Passed, Failed, In Progress
    public DateTime SubmittedAt { get; set; }
    public bool IsGraded { get; set; }
    public string? AudioUrl { get; set; }
    public string? Transcript { get; set; }
    public string? EssayText { get; set; }
    public string? AiFeedback { get; set; }
}

public class AdminSubmissionStatsDto
{
    public int TotalSubmissions { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public int InProgressCount { get; set; }
    public double AverageScore { get; set; }
    public List<DailySubmissionStatsDto> DailyStats { get; set; } = new();
}

public class DailySubmissionStatsDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public double AverageScore { get; set; }
}