// Examify.Application/DTOs/Exercises/SpeakingQuestionDto.cs
namespace Examify.Application.DTOs.Exercises;

public class SpeakingQuestionDto
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int PreparationTime { get; set; }
    public int SpeakingTime { get; set; }
    public string? SampleAnswer { get; set; }
}