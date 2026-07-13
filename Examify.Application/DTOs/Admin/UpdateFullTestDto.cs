// Examify.Application/DTOs/Admin/UpdateFullTestDto.cs
namespace Examify.Application.DTOs.Admin;

public class UpdateFullTestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int Difficulty { get; set; }
    public Guid? ReadingExerciseId { get; set; }
    public Guid? ListeningExerciseId { get; set; }
    public Guid? WritingExerciseId { get; set; }
    public Guid? SpeakingExerciseId { get; set; }
    public bool IsFree { get; set; }
    public decimal Price { get; set; }
}