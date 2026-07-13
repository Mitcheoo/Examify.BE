namespace Examify.Application.DTOs.Submissions;

public class MySubmissionItemDto
{
    public Guid SubmissionId { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int SkillType { get; set; }           // ✅ THÊM
    public string SkillName { get; set; } = string.Empty;  // ✅ THÊM
    public short TotalScore { get; set; }
    public int TotalQuestions { get; set; }      // ✅ THÊM
    public int CorrectCount { get; set; }        // ✅ THÊM
    public int TimeSpentSeconds { get; set; }    // ✅ THÊM
    public DateTime SubmittedAt { get; set; }

    // Cho Speaking
    public string? AudioUrl { get; set; }
    public string? Transcript { get; set; }

    // Cho Writing
    public string? EssayText { get; set; }
}