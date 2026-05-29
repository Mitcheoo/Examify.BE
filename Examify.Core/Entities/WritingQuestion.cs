// Examify.Core/Entities/WritingQuestion.cs
using Examify.Core.Entities;

namespace Examify.Core.Entities;

public class WritingQuestion : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public int OrderNumber { get; set; }        // ✅ THÊM DÒNG NÀY
    public int TaskType { get; set; }
    public string PromptText { get; set; } = string.Empty;
    public string? SampleImageUrl { get; set; }
    public string? ModelAnswer { get; set; }
    public string? RubricJson { get; set; }
    public int MinWords { get; set; } = 150;
    public int MaxWords { get; set; } = 300;
    public int RecommendedTimeMinutes { get; set; } = 20;

    public virtual Exercise Exercise { get; set; } = null!;
}