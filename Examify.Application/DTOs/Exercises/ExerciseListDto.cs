// Examify.Application/DTOs/Exercises/ExerciseListDto.cs
namespace Examify.Application.DTOs.Exercises;

public class ExerciseListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Skill { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int Difficulty { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int AttemptCount { get; set; }
    public bool IsCompleted { get; set; }
    public double? LastScore { get; set; }
}