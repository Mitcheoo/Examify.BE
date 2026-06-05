// Examify.Application/DTOs/Exercises/ExerciseDetailDto.cs
namespace Examify.Application.DTOs.Exercises;

public class ExerciseDetailDto
{
    public Guid Id { get; set; }
    public int Skill { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AudioUrl { get; set; }
    public string? Passage { get; set; }
    public int TotalParts { get; set; }
    public int TotalQuestions { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int Difficulty { get; set; }
    public int AttemptCount { get; set; }
    public bool IsPurchased { get; set; }
    public DateTime CreatedAt { get; set; }
}