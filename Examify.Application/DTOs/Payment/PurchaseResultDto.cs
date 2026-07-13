// Examify.Application/DTOs/Payment/PurchaseResultDto.cs
namespace Examify.Application.DTOs.Payment;

public class PurchaseResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal NewBalance { get; set; }
    public Guid ExerciseId { get; set; }
}