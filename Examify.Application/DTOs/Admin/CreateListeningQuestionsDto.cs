// Examify.Application/DTOs/Admin/CreateListeningQuestionsDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreateListeningQuestionsDto
{
    public List<CreateListeningQuestionDto> Questions { get; set; } = new();
}

public class CreateListeningQuestionDto
{
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
}