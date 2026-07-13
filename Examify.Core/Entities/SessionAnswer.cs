// Examify.Core/Entities/SessionAnswer.cs
namespace Examify.Core.Entities;

public class SessionAnswer : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid QuestionId { get; set; }
    public int SkillType { get; set; }
    public string? UserAnswer { get; set; }
    public string? AudioUrl { get; set; }
    public string? Transcript { get; set; }
    public bool IsSubmitted { get; set; } = false;
    // Navigation
    public virtual FullTestSession Session { get; set; } = null!;
}