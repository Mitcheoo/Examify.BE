// Examify.Application/DTOs/Exercises/ReadingExamDto.cs
namespace Examify.Application.DTOs.Exercises;

public class ReadingExamDto
{
    public Guid ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; }
    public int TotalQuestions { get; set; }
    public List<PartDto> Parts { get; set; } = new();
    public List<ReadingQuestionDto> Questions { get; set; } = new();
}