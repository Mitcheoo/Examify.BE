// Examify.Application/DTOs/Admin/CreateWritingQuestionsDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreateWritingQuestionsDto
{
    public List<CreateWritingQuestionDto> Questions { get; set; } = new();
}

public class CreateWritingQuestionDto
{
    public int TaskType { get; set; } // 1=Task 1, 2=Task 2
    public int OrderNumber { get; set; }
    public string PromptText { get; set; } = string.Empty;
    public int MinWords { get; set; } = 150;
    public int MaxWords { get; set; } = 300;
    public int RecommendedTimeMinutes { get; set; } = 20;
    public string? SampleImageUrl { get; set; }
    public string? ModelAnswer { get; set; }
    public string? RubricJson { get; set; }
}