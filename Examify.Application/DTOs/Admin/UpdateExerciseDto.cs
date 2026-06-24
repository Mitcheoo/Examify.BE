// Examify.Application/DTOs/Admin/UpdateExerciseDto.cs
namespace Examify.Application.DTOs.Admin;

public class UpdateExerciseDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalParts { get; set; }
    public int TotalQuestions { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int Difficulty { get; set; }

    public string? Source { get; set; }
}