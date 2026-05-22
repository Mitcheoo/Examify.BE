// Examify.Core/Enums/TransactionStatus.cs
namespace Examify.Core.Enums;

public enum TransactionStatus
{
    Pending = 0,    // Đang xử lý
    Completed = 1,  // Đã hoàn thành
    Failed = 2      // Thất bại
}