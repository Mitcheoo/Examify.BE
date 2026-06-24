// Examify.Application/DTOs/Exercises/AnswerDetailDto.cs
namespace Examify.Application.DTOs.Exercises;

public class AnswerDetailDto
{
    public Guid QuestionId { get; set; }
    public string? UserAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}