// Examify.Application/Cqrs/Commands/Payment/CapturePayPalOrderCommandHandler.cs
using Examify.Application.DTOs.Payment;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using Examify.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Examify.Application.Cqrs.Commands.Payment;

public class CapturePayPalOrderCommandHandler : IRequestHandler<CapturePayPalOrderCommand, CapturePayPalOrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayPalService _paypalService;
    private readonly ILogger<CapturePayPalOrderCommandHandler> _logger;

    public CapturePayPalOrderCommandHandler(
        IUnitOfWork unitOfWork,
        PayPalService paypalService,
        ILogger<CapturePayPalOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paypalService = paypalService;
        _logger = logger;
    }

    public async Task<CapturePayPalOrderResponse> Handle(CapturePayPalOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📥 [CapturePayPalOrder] Bắt đầu xử lý capture cho order: {PayPalOrderId}", request.PayPalOrderId);

        // ============================================================
        // 1. TÌM TRANSACTION
        // ============================================================
        _logger.LogInformation("🔍 Đang tìm transaction với PayPalOrderId: {PayPalOrderId}", request.PayPalOrderId);

        var transaction = (await _unitOfWork.Transactions
            .FindAsync(t => t.PayPalOrderId == request.PayPalOrderId && t.UserId == request.UserId))
            .FirstOrDefault();

        if (transaction is null)
        {
            _logger.LogWarning("❌ Không tìm thấy transaction cho order: {PayPalOrderId}", request.PayPalOrderId);
            throw new NotFoundException($"Không tìm thấy giao dịch với PayPalOrderId: {request.PayPalOrderId}");
        }

        _logger.LogInformation("✅ Tìm thấy transaction: Id={TransactionId}, Status={Status}, Amount={Amount}",
            transaction.Id, transaction.Status, transaction.Amount);

        if (transaction.Status == "Success")
        {
            _logger.LogWarning("⚠️ Transaction đã được xử lý trước đó: {TransactionId}", transaction.Id);
            throw new InvalidOperationException("Giao dịch đã được xử lý");
        }

        if (transaction.Status == "Failed")
        {
            _logger.LogWarning("⚠️ Transaction đã thất bại trước đó: {TransactionId}", transaction.Id);
            throw new InvalidOperationException("Giao dịch đã thất bại, vui lòng tạo giao dịch mới");
        }

        // ============================================================
        // 2. CAPTURE TRÊN PAYPAL
        // ============================================================
        _logger.LogInformation("🔄 Đang gọi PayPal API để capture order: {PayPalOrderId}", request.PayPalOrderId);

        var (isSuccess, status, captureId, captureStatus, error) =
            await _paypalService.CaptureOrderAsync(request.PayPalOrderId);

        if (!isSuccess)
        {
            _logger.LogError("❌ Capture thất bại: {Error}", error);

            // Cập nhật transaction thành Failed
            transaction.Status = "Failed";
            transaction.Description += $"\nLỗi: {error}";
            transaction.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Transactions.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            throw new InvalidOperationException($"Không thể capture thanh toán: {error}");
        }

        _logger.LogInformation("✅ PayPal capture thành công: Status={Status}, CaptureId={CaptureId}",
            status, captureId);

        // ============================================================
        // 3. KIỂM TRA TRẠNG THÁI COMPLETED
        // ============================================================
        if (!string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("⚠️ Thanh toán chưa hoàn tất: Status={Status}", status);

            // Cập nhật transaction thành Failed
            transaction.Status = "Failed";
            transaction.Description += $"\nTrạng thái PayPal: {status}";
            transaction.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Transactions.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            throw new InvalidOperationException($"Thanh toán chưa hoàn tất. Trạng thái từ PayPal: {status}");
        }

        // ============================================================
        // 4. LẤY VÍ CỦA USER
        // ============================================================
        _logger.LogInformation("💰 Đang lấy ví của user: {UserId}", request.UserId);

        var wallet = (await _unitOfWork.Wallets
            .FindAsync(w => w.UserId == request.UserId && !w.IsDeleted))
            .FirstOrDefault();

        if (wallet is null)
        {
            _logger.LogInformation("🆕 User chưa có ví, đang tạo ví mới cho user: {UserId}", request.UserId);

            wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Balance = 0,
                TotalDeposited = 0,
                TotalSpent = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Wallets.AddAsync(wallet);
        }

        _logger.LogInformation("💰 Số dư hiện tại của ví: {Balance} VND", wallet.Balance);

        // ============================================================
        // 5. CẬP NHẬT SỐ DƯ
        // ============================================================
        var balanceBefore = wallet.Balance;
        wallet.Balance += transaction.Amount;
        wallet.TotalDeposited += transaction.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("💰 Cập nhật số dư: {BalanceBefore} → {BalanceAfter} (+{Amount})",
            balanceBefore, wallet.Balance, transaction.Amount);

        // ============================================================
        // 6. CẬP NHẬT TRANSACTION
        // ============================================================
        transaction.Status = "Success";
        transaction.BalanceBefore = balanceBefore;
        transaction.BalanceAfter = wallet.Balance;
        transaction.PayPalCaptureId = captureId;
        transaction.UpdatedAt = DateTime.UtcNow;
        transaction.Description = $"Nạp {transaction.Amount:N0} VND thành công qua PayPal. CaptureId: {captureId}";

        _logger.LogInformation("📝 Cập nhật transaction: Status=Success, BalanceBefore={BalanceBefore}, BalanceAfter={BalanceAfter}",
            balanceBefore, wallet.Balance);

        // ============================================================
        // 7. LƯU TẤT CẢ VÀO DATABASE
        // ============================================================
        _logger.LogInformation("💾 Đang lưu dữ liệu vào database...");

        await _unitOfWork.Wallets.UpdateAsync(wallet);
        await _unitOfWork.Transactions.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("✅ Lưu dữ liệu thành công!");

        // ============================================================
        // 8. TRẢ VỀ KẾT QUẢ
        // ============================================================
        var amountUSD = _paypalService.ConvertVNDToUSD(transaction.Amount);

        _logger.LogInformation("📤 Trả về kết quả: AmountVND={AmountVND}, AmountUSD={AmountUSD}, NewBalance={NewBalance}",
            transaction.Amount, amountUSD, wallet.Balance);

        return new CapturePayPalOrderResponse
        {
            PayPalOrderId = request.PayPalOrderId,
            Status = status,
            CaptureId = captureId,
            CaptureStatus = captureStatus,
            AmountVND = transaction.Amount,
            AmountUSD = amountUSD,
            NewBalance = wallet.Balance
        };
    }
}