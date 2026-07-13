// Examify.Application/DTOs/Admin/CreateFullTestDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreateFullTestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimitSeconds { get; set; } = 7200;
    public int Difficulty { get; set; } = 2;

    public Guid? ReadingExerciseId { get; set; }
    public Guid? ListeningExerciseId { get; set; }
    public Guid? WritingExerciseId { get; set; }
    public Guid? SpeakingExerciseId { get; set; }
    public bool IsFree { get; set; } = true;
    public decimal Price { get; set; } = 0;
}