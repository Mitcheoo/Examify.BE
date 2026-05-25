// Examify.Application/DTOs/Exercises/SpeakingExamDto.cs
namespace Examify.Application.DTOs.Exercises;

public class SpeakingExamDto
{
    public Guid ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; }
    public int TotalQuestions { get; set; }
    public List<SpeakingPartDto> Parts { get; set; } = new();
    public List<SpeakingQuestionDto> Questions { get; set; } = new();
}

public class SpeakingPartDto
{
    public int PartNumber { get; set; }
    public string PartTitle { get; set; } = string.Empty;
    public int PreparationTime { get; set; }
    public int SpeakingTime { get; set; }
    public List<SpeakingQuestionDto> Questions { get; set; } = new();
}