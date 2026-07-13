// Examify.Core/Entities/Transaction.cs
namespace Examify.Core.Entities;

public class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? WalletId { get; set; }
    public Guid? ExerciseId { get; set; }
    public Guid? SubmissionId { get; set; }

    public decimal Amount { get; set; }            // Số tiền (VND, + nạp, - mua)
    public decimal BalanceBefore { get; set; }     // Số dư trước
    public decimal BalanceAfter { get; set; }      // Số dư sau

    public string Type { get; set; } = string.Empty; // "Deposit", "Purchase", "Refund"
    public string Status { get; set; } = "Pending";  // "Pending", "Success", "Failed"
    public string? Description { get; set; }
    
    public string? PaymentMethod { get; set; }     // "PayPal", "VNPAY", "MOMO"
    public string? PayPalOrderId { get; set; }     // PayPal Order ID
    public string? PayPalCaptureId { get; set; }   // PayPal Capture ID

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Wallet? Wallet { get; set; }
    public virtual Exercise? Exercise { get; set; }
}