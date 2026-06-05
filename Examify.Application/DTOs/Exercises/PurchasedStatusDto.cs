// Examify.Application/DTOs/Exercises/PurchasedStatusDto.cs
namespace Examify.Application.DTOs.Exercises;

public class PurchasedStatusDto
{
    public bool IsPurchased { get; set; }
    public DateTime? PurchasedAt { get; set; }
    public Guid? TransactionId { get; set; }
}