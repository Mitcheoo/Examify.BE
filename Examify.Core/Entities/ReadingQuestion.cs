// Examify.Core/Entities/ReadingQuestion.cs
namespace Examify.Core.Entities;

public class ReadingQuestion : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public int PartNumber { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "multiple_choice";
    public string OptionsJson { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }

    // Navigation
    public virtual Exercise Exercise { get; set; } = null!;
}