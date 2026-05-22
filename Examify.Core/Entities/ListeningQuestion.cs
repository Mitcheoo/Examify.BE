// Examify.Core/Entities/ListeningQuestion.cs
namespace Examify.Core.Entities;

public class ListeningQuestion : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public int PartNumber { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Explanation { get; set; }

    // Navigation
    public virtual Exercise Exercise { get; set; } = null!;
}