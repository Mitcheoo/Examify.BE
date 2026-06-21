// Examify.Application/DTOs/Session/SaveAnswerRequestDto.cs
namespace Examify.Application.DTOs.Session;

public class SaveAnswerRequestDto
{
    public Guid SessionId { get; set; }
    public List<QuestionAnswerDto> Answers { get; set; } = new();
}

public class QuestionAnswerDto
{
    public Guid QuestionId { get; set; }
    public int SkillType { get; set; }
    public string? UserAnswer { get; set; }
    public string? AudioUrl { get; set; }
    public string? Transcript { get; set; }
}
public class SessionAnswerDto
{
    public Guid QuestionId { get; set; }
    public int SkillType { get; set; }
    public string? UserAnswer { get; set; }
    public string? AudioUrl { get; set; }
    public string? Transcript { get; set; }
    public DateTime UpdatedAt { get; set; }
}