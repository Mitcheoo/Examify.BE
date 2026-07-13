// Examify.Core/Entities/PurchasedExercise.cs
namespace Examify.Core.Entities;

public class PurchasedExercise : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ExerciseId { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Exercise Exercise { get; set; } = null!;
}