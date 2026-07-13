// Examify.Application/DTOs/Exercises/ExerciseStatsDto.cs
namespace Examify.Application.DTOs.Exercises;
//CHO EXERCISE
public class ExerciseStatsDto
{
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int Skill { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public short BestScore { get; set; }
    public double AverageScore { get; set; }
    public short LastScore { get; set; }
    public DateTime? LastSubmittedAt { get; set; }
}