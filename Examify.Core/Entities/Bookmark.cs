// Examify.Core/Entities/Bookmark.cs
namespace Examify.Core.Entities;

public class Bookmark : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public int SkillType { get; set; }
    public string? Note { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
}