// Examify.Core/Entities/Part.cs
namespace Examify.Core.Entities;

public class Part : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public int PartNumber { get; set; } // 1, 2, 3
    public string? Title { get; set; }
    public string? Passage { get; set; }
    public string? AudioUrl { get; set; }

    // Navigation
    public virtual Exercise Exercise { get; set; } = null!;
}