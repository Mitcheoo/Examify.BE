// Examify.Core/Interfaces/IWalletService.cs
using Examify.Core.Entities;
using Examify.Core.Enums;

namespace Examify.Core.Interfaces;

public interface IWalletService
{
    /// <summary>
    /// Lấy thông tin ví của user
    /// </summary>
    Task<Wallet?> GetWalletByUserIdAsync(Guid userId);

    /// <summary>
    /// Lấy số dư hiện tại của user
    /// </summary>
    Task<decimal> GetBalanceAsync(Guid userId);

    /// <summary>
    /// Nạp tiền vào ví
    /// </summary>
    Task<Transaction> DepositAsync(Guid userId, decimal amount, string description, string? paymentMethod = null, string? externalTransactionId = null);

    /// <summary>
    /// Trừ tiền từ ví (mua bài thi)
    /// </summary>
    Task<Transaction> PurchaseAsync(Guid userId, decimal amount, string description, Guid? exerciseId = null);

    /// <summary>
    /// Hoàn tiền vào ví
    /// </summary>
    Task<Transaction> RefundAsync(Guid userId, decimal amount, string description, string? referenceId = null);

    /// <summary>
    /// Lấy lịch sử giao dịch của user
    /// </summary>
    Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(Guid userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Lấy lịch sử giao dịch với filter
    /// </summary>
    Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(
        Guid userId,
        TransactionType? type = null,
        TransactionStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Lấy tổng số tiền đã nạp
    /// </summary>
    Task<decimal> GetTotalDepositedAsync(Guid userId);

    /// <summary>
    /// Lấy tổng số tiền đã chi
    /// </summary>
    Task<decimal> GetTotalSpentAsync(Guid userId);

    /// <summary>
    /// Kiểm tra user có đủ tiền không
    /// </summary>
    Task<bool> HasSufficientBalanceAsync(Guid userId, decimal amount);

    /// <summary>
    /// Lấy transaction theo ID
    /// </summary>
    Task<Transaction?> GetTransactionByIdAsync(Guid transactionId);

    /// <summary>
    /// Cập nhật trạng thái transaction
    /// </summary>
    Task UpdateTransactionStatusAsync(Guid transactionId, TransactionStatus status, string? externalTransactionId = null);

    /// <summary>
    /// Tạo wallet cho user mới (nếu chưa có)
    /// </summary>
    Task<Wallet> CreateWalletForUserAsync(Guid userId);
}