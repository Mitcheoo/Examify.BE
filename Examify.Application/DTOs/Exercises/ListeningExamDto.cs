// Examify.Application/DTOs/Exercises/ListeningExamDto.cs
namespace Examify.Application.DTOs.Exercises;

public class ListeningExamDto
{
    public Guid ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalParts { get; set; }
    public List<ListeningPartDto> Parts { get; set; } = new();
    public List<ListeningQuestionDto> Questions { get; set; } = new();
}

public class ListeningPartDto
{
    public int PartNumber { get; set; }
    public string? AudioUrl { get; set; }
    public List<ListeningQuestionDto> Questions { get; set; } = new();
}