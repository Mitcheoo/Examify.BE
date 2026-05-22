// Examify.Core/Entities/Transaction.cs
namespace Examify.Core.Entities;

public class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? SubmissionId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? IdempotencyKey { get; set; }
    public string? Hash { get; set; }
    public string? Description { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Submission? Submission { get; set; }
}