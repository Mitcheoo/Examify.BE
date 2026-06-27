// Examify.Application/DTOs/Submissions/SubmissionHistoryDto.cs
namespace Examify.Application.DTOs.Submissions;

public class SubmissionHistoryDto
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int Skill { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public double TotalScore { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public DateTime SubmittedAt { get; set; }
}