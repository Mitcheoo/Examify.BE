// Examify.Application/DTOs/Exercises/ExerciseProgressDto.cs
namespace Examify.Application.DTOs.Exercises;

public class ExerciseProgressDto
{
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public short BestScore { get; set; }
    public short WorstScore { get; set; }
    public short AverageScore { get; set; }
    public double Improvement { get; set; }          // Tiến bộ (điểm tăng/giảm)
    public int TotalTimeSpent { get; set; }          // Tổng thời gian làm bài (giây)
    public List<WeeklyProgressDto> WeeklyProgress { get; set; } = new();
}

public class WeeklyProgressDto
{
    public int Week { get; set; }
    public int Year { get; set; }
    public short AverageScore { get; set; }
    public int Attempts { get; set; }
}