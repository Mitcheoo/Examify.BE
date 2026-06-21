// Examify.Application/DTOs/Exercises/QuestionDto.cs
namespace Examify.Application.DTOs.Exercises;

public class QuestionDto
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionType { get; set; } = "multiple_choice";
    public string QuestionText { get; set; } = string.Empty;
    public Dictionary<string, string>? Options { get; set; }
    public string? AudioUrl { get; set; }  // Cho Listening
}

public class WritingQuestionDto
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public int TaskType { get; set; } // 1=Task 1, 2=Task 2
    public string PromptText { get; set; } = string.Empty;
    public int MinWords { get; set; }
    public int MaxWords { get; set; }
    public int RecommendedTimeMinutes { get; set; }

}

public class SpeakingQuestionDto
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public int PartNumber { get; set; } // 1, 2, 3
    public string QuestionText { get; set; } = string.Empty;
    public int PreparationTime { get; set; }
    public int SpeakingTime { get; set; }
}
