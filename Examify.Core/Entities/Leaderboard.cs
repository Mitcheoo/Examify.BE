// Examify.Core/Entities/Leaderboard.cs
namespace Examify.Core.Entities;

public class Leaderboard : BaseEntity
{
    public Guid UserId { get; set; }
    public int SkillType { get; set; }
    public int TotalScore { get; set; }
    public int TotalAttempts { get; set; }
    public double AverageScore { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
}