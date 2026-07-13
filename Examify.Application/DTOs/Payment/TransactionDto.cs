
// Examify.Application/DTOs/Payment/TransactionDto.cs
namespace Examify.Application.DTOs.Payment;

public class TransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
}