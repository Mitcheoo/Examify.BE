// Examify.Core/Entities/Wallet.cs
namespace Examify.Core.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; } = 0;           // Số dư hiện tại (VND)
    public decimal TotalDeposited { get; set; } = 0;    // Tổng nạp (VND)
    public decimal TotalSpent { get; set; } = 0;        // Tổng chi (VND)
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}