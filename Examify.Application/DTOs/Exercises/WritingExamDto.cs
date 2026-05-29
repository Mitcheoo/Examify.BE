// Examify.Application/DTOs/Exercises/WritingExamDto.cs
namespace Examify.Application.DTOs.Exercises;

public class WritingExamDto
{
    public Guid ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; }
    public List<WritingQuestionDto> Questions { get; set; } = new();
}