// Examify.Core/Entities/Wallet.cs
using System.Transactions;

namespace Examify.Core.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; } = 0;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}