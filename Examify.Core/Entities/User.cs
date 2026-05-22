// Examify.Core/Entities/User.cs
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Examify.Core.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? AvatarUrl { get; set; }

    // Navigation properties
    public virtual Wallet? Wallet { get; set; }
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<FullTestSession> FullTestSessions { get; set; } = new List<FullTestSession>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();
}