// Examify.Application/DTOs/Payment/PurchasedExerciseDto.cs
namespace Examify.Application.DTOs.Payment;

public class PurchasedExerciseDto
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Skill { get; set; }
    public bool IsFullTest { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PurchasedAt { get; set; }
}