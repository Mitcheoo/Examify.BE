// Examify.Application/DTOs/Admin/CreateReadingQuestionsDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreateReadingQuestionsDto
{
    public List<CreateReadingQuestionDto> Questions { get; set; } = new();
}

public class CreateReadingQuestionDto
{
    public int PartNumber { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionType { get; set; } = "multiple_choice";
    public string QuestionText { get; set; } = string.Empty;
    public string? Passage { get; set; } = string.Empty;  // ✅ THÊM DÒNG NÀY
    public Dictionary<string, string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
}