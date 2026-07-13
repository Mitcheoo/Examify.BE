// Examify.Application/DTOs/Payment/WalletDto.cs
namespace Examify.Application.DTOs.Payment;

public class WalletDto
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalDeposited { get; set; }
    public decimal TotalSpent { get; set; }
    public bool IsActive { get; set; }
}