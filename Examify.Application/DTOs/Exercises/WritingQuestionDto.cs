// Examify.Application/DTOs/Exercises/WritingQuestionDto.cs
namespace Examify.Application.DTOs.Exercises;

public class WritingQuestionDto
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public int TaskType { get; set; } // 1: Task 1, 2: Task 2
    public string PromptText { get; set; } = string.Empty;
    public string? SampleImageUrl { get; set; }
    public string? ModelAnswer { get; set; }
    public string? RubricJson { get; set; }
    public int MinWords { get; set; }
    public int MaxWords { get; set; }
    public int RecommendedTimeMinutes { get; set; }
}