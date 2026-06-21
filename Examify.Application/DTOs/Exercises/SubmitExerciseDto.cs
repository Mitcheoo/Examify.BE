/*// Examify.Application/DTOs/Exercises/SubmitExerciseDto.cs
namespace Examify.Application.DTOs.Exercises;

public class SubmitExerciseDto
{
    public List<AnswerDto> Answers { get; set; } = new();
    public int TimeSpentSeconds { get; set; }
}

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    public string? SelectedOption { get; set; }  // Cho Reading/Listening
    public string? EssayText { get; set; }       // Cho Writing
    public string? AudioBase64 { get; set; }     // Cho Speaking
}*/