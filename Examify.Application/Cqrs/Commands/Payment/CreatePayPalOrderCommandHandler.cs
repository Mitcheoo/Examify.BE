// Examify.Application/Cqrs/Commands/Payment/CreatePayPalOrderCommandHandler.cs

using Examify.Application.DTOs.Payment;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Examify.Application.Cqrs.Commands.Payment;

public class CreatePayPalOrderCommandHandler : IRequestHandler<CreatePayPalOrderCommand, CreatePayPalOrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayPalService _paypalService;
    private readonly ILogger<CreatePayPalOrderCommandHandler> _logger;

    public CreatePayPalOrderCommandHandler(
        IUnitOfWork unitOfWork,
        PayPalService paypalService,
        ILogger<CreatePayPalOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paypalService = paypalService;
        _logger = logger;
    }

    public async Task<CreatePayPalOrderResponse> Handle(CreatePayPalOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📝 [CreateOrder] Bắt đầu tạo đơn hàng cho user: {UserId}, Amount: {Amount} VND",
            request.UserId, request.AmountVND);

        // 1. Kiểm tra số tiền
        if (request.AmountVND <= 0)
            throw new ArgumentException("Số tiền phải lớn hơn 0");

        // 2. Tạo mã đơn hàng
        var orderCode = $"DEP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 30);
        _logger.LogInformation("📋 OrderCode: {OrderCode}", orderCode);

        // 3. Quy đổi VND → USD
        var amountUSD = _paypalService.ConvertVNDToUSD(request.AmountVND);
        _logger.LogInformation("💰 Amount USD: {AmountUSD}", amountUSD);

        // 4. Tạo order trên PayPal
        _logger.LogInformation("🔄 Đang gọi PayPal API tạo order...");
        var (isSuccess, paypalOrderId, approvalUrl, status, error) =
            await _paypalService.CreateOrderAsync(orderCode, amountUSD);

        if (!isSuccess)
        {
            _logger.LogError("❌ PayPal tạo order thất bại: {Error}", error);
            throw new InvalidOperationException($"Không thể tạo đơn hàng PayPal: {error}");
        }

        _logger.LogInformation("✅ PayPal tạo order thành công: {PayPalOrderId}", paypalOrderId);

        // ============================================================
        // ✅ 5. TẠO TRANSACTION TRONG DATABASE
        // ============================================================
        _logger.LogInformation("📝 Đang tạo Transaction trong database...");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Amount = request.AmountVND,
            BalanceBefore = 0,
            BalanceAfter = 0,
            Type = "Deposit",
            Status = "Pending",
            Description = $"Nạp {request.AmountVND:N0} VND qua PayPal. OrderCode: {orderCode}",
            PaymentMethod = "PayPal",
            PayPalOrderId = paypalOrderId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("✅ Transaction đã được tạo: {TransactionId}", transaction.Id);

        // 6. Trả về response
        return new CreatePayPalOrderResponse
        {
            OrderCode = orderCode,
            PayPalOrderId = paypalOrderId,
            ApprovalUrl = approvalUrl,
            Status = status,
            AmountVND = request.AmountVND,
            AmountUSD = amountUSD
        };
    }
}