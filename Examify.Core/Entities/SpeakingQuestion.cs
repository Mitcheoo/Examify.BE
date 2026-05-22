// Examify.Core/Entities/SpeakingQuestion.cs
namespace Examify.Core.Entities;

public class SpeakingQuestion : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public int PartNumber { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int PreparationTime { get; set; } = 30;
    public int SpeakingTime { get; set; } = 60;
    public string? SampleAnswer { get; set; }

    // Navigation
    public virtual Exercise Exercise { get; set; } = null!;
}