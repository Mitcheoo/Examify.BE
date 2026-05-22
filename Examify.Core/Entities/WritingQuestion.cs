// Examify.Core/Entities/WritingQuestion.cs
namespace Examify.Core.Entities;

public class WritingQuestion : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public int TaskType { get; set; } // 1: Task 1, 2: Task 2
    public string PromptText { get; set; } = string.Empty;
    public string? SampleImageUrl { get; set; }
    public string? ModelAnswer { get; set; }
    public string? RubricJson { get; set; }
    public int MinWords { get; set; } = 150;
    public int MaxWords { get; set; } = 300;
    public int RecommendedTimeMinutes { get; set; } = 20;

    // Navigation
    public virtual Exercise Exercise { get; set; } = null!;
}