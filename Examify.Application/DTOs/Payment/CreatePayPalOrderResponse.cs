// Examify.Application/DTOs/Payment/CreatePayPalOrderResponse.cs
namespace Examify.Application.DTOs.Payment;

public class CreatePayPalOrderResponse
{
    public string OrderCode { get; set; } = string.Empty;
    public string? PayPalOrderId { get; set; }
    public string? ApprovalUrl { get; set; }
    public string? Status { get; set; }
    public decimal AmountVND { get; set; }
    public decimal AmountUSD { get; set; }
}