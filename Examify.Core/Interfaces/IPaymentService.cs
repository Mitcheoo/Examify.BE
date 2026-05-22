// Examify.Core/Interfaces/IPaymentService.cs
namespace Examify.Core.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Tạo URL thanh toán (giả lập hoặc VnPay)
    /// </summary>
    Task<string> CreatePaymentUrlAsync(decimal amount, string orderDescription, string returnUrl);

    /// <summary>
    /// Xác nhận thanh toán
    /// </summary>
    Task<PaymentResult> VerifyPaymentAsync(IDictionary<string, string> queryParams);

    /// <summary>
    /// Xử lý nạp tiền trực tiếp (mock)
    /// </summary>
    Task<PaymentResult> ProcessDepositAsync(Guid userId, decimal amount);
}

/// <summary>
/// Kết quả thanh toán
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PaymentUrl { get; set; }
}