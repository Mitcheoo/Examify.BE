/*// Examify.Application/DTOs/Exercises/ExerciseResultDto.cs
namespace Examify.Application.DTOs.Exercises;

public class ExerciseResultDto
{
    public Guid SubmissionId { get; set; }
    public Guid ExerciseId { get; set; }
    public int Skill { get; set; }
    public double TotalScore { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public List<AnswerDetailDto> Details { get; set; } = new();
    public object? AiFeedback { get; set; }  // Cho Writing/Speaking
}*/