// Examify.Application/DTOs/Admin/CreateExerciseDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreateExerciseDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Skill { get; set; } // 0=Reading, 1=Listening, 2=Writing, 3=Speaking
    public int TotalParts { get; set; } = 3;
    public int TotalQuestions { get; set; }
    public int TimeLimitSeconds { get; set; } = 3600;
    public int Difficulty { get; set; } = 1;

    public string? Source { get; set; }
}