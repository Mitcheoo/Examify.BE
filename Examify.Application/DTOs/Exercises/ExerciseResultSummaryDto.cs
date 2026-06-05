// Examify.Application/DTOs/Exercises/ExerciseResultSummaryDto.cs
namespace Examify.Application.DTOs.Exercises;

public class ExerciseResultSummaryDto
{
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }           // Tổng số lần làm
    public short HighestScore { get; set; }          // Điểm cao nhất
    public short AverageScore { get; set; }          // Điểm trung bình
    public short LastScore { get; set; }             // Điểm lần gần nhất
    public DateTime? LastAttemptDate { get; set; }   // Ngày làm gần nhất
    public List<ScoreHistoryDto> ScoreHistory { get; set; } = new();
}

public class ScoreHistoryDto
{
    public DateTime SubmittedAt { get; set; }
    public short Score { get; set; }
    public int TimeSpentSeconds { get; set; }
}