// Examify.Application/DTOs/Payment/CapturePayPalOrderRequest.cs
namespace Examify.Application.DTOs.Payment;

public class CapturePayPalOrderRequest
{
    public string PayPalOrderId { get; set; } = string.Empty;
}
