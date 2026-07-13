// Examify.Application/DTOs/Payment/CapturePayPalOrderResponse.cs
namespace Examify.Application.DTOs.Payment;

public class CapturePayPalOrderResponse
{
    public string PayPalOrderId { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? CaptureId { get; set; }
    public string? CaptureStatus { get; set; }
    public decimal AmountVND { get; set; }
    public decimal AmountUSD { get; set; }
    public decimal NewBalance { get; set; }
}