// Examify.Application/DTOs/Payment/CreatePayPalOrderRequest.cs
namespace Examify.Application.DTOs.Payment;
public class CreatePayPalOrderRequest
{
    public Guid ExerciseId { get; set; }
    public decimal AmountVND { get; set; }
    public string? OrderCode { get; set; }
}