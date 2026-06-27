// Examify.Application/DTOs/Admin/CreateSpeakingQuestionsDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreateSpeakingQuestionsDto
{
    public List<CreateSpeakingQuestionDto> Questions { get; set; } = new();
}

public class CreateSpeakingQuestionDto
{
    public int PartNumber { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int PreparationTime { get; set; } = 30;
    public int SpeakingTime { get; set; } = 60;
    public string? SampleAnswer { get; set; }
    public string? AudioUrl { get; set; }
}