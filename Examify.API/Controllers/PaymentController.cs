// Examify.API/Controllers/PaymentController.cs

using Examify.Application.Cqrs.Commands.Payment;
using Examify.Application.Cqrs.Queries.Payment;
using Examify.Application.DTOs.Payment;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    private Guid GetUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                          User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID");

        return userId;
    }

    // ============================================================
    // 1. TẠO ĐƠN HÀNG PAYPAL
    // ============================================================
    /// <summary>
    /// Tạo đơn hàng thanh toán qua PayPal
    /// </summary>
    [HttpPost("create-order")]
    public async Task<ActionResult<CreatePayPalOrderResponse>> CreateOrder([FromBody] CreatePayPalOrderRequest request)
    {
        var userId = GetUserId();

        // Validate
        if (request.AmountVND <= 0)
            return BadRequest(new { message = "Số tiền phải lớn hơn 0" });

        var command = new CreatePayPalOrderCommand(userId, request);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    // ============================================================
    // 2. CAPTURE PAYPAL ORDER
    // ============================================================
    /// <summary>
    /// Xác nhận và capture thanh toán PayPal
    /// </summary>
    [HttpPost("capture-order/{orderCode}")]
    public async Task<ActionResult<CapturePayPalOrderResponse>> CaptureOrder(
        string orderCode,
        [FromBody] CapturePayPalOrderRequest request)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(orderCode))
            return BadRequest(new { message = "OrderCode là bắt buộc" });

        if (string.IsNullOrWhiteSpace(request.PayPalOrderId))
            return BadRequest(new { message = "PayPalOrderId là bắt buộc" });

        var command = new CapturePayPalOrderCommand(userId, orderCode, request);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    // ============================================================
    // 3. KIỂM TRA TRẠNG THÁI GIAO DỊCH
    // ============================================================
    /// <summary>
    /// Kiểm tra trạng thái của một giao dịch
    /// </summary>
    [HttpGet("transaction/{orderCode}")]
    public async Task<IActionResult> GetTransactionStatus(string orderCode)
    {
        var userId = GetUserId();

        // Tìm transaction theo OrderCode
        var transactions = await _unitOfWork.Transactions
            .FindAsync(t => t.UserId == userId && !t.IsDeleted);

        var transaction = transactions
            .Where(t => t.Description != null && t.Description.Contains(orderCode))
            .FirstOrDefault();

        if (transaction is null)
            return NotFound(new { message = "Không tìm thấy giao dịch" });

        return Ok(new
        {
            transaction.Id,
            transaction.Status,
            transaction.Amount,
            transaction.Type,
            transaction.Description,
            transaction.CreatedAt,
            transaction.PaymentMethod
        });
    }

    // ============================================================
    // 4. MUA BÀI THI
    // ============================================================
    /// <summary>
    /// Mua bài thi bằng số dư trong ví
    /// </summary>
    [HttpPost("purchase")]
    public async Task<ActionResult<PurchaseResultDto>> PurchaseExercise([FromBody] PurchaseRequest request)
    {
        var userId = GetUserId();

        if (request.ExerciseId == Guid.Empty)
            return BadRequest(new { message = "ExerciseId là bắt buộc" });

        var command = new PurchaseExerciseCommand(userId, request);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    // ============================================================
    // 5. GET ALL TRANSACTIONS (ADMIN)
    // ============================================================
    /// <summary>
    /// Lấy tất cả giao dịch (Admin)
    /// </summary>
    [HttpGet("admin/transactions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null)
    {
        var transactions = await _unitOfWork.Transactions
            .FindAsync(t => !t.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            transactions = transactions.Where(t => t.Status == status);
        }

        if (!string.IsNullOrEmpty(type))
        {
            transactions = transactions.Where(t => t.Type == type);
        }

        var totalCount = transactions.Count();
        var items = transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.UserId,
                t.Amount,
                t.BalanceBefore,
                t.BalanceAfter,
                t.Type,
                t.Status,
                t.Description,
                t.PaymentMethod,
                t.CreatedAt,
                t.UpdatedAt
            })
            .ToList();

        // Lấy thông tin user cho từng transaction
        var result = new List<object>();
        foreach (var item in items)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(item.UserId);
            result.Add(new
            {
                item.Id,
                TransactionId = item.Id.ToString().Substring(0, 12).ToUpper(),
                UserName = user?.FullName ?? "Unknown",
                UserEmail = user?.Email ?? "unknown@email.com",
                item.Amount,
                item.BalanceBefore,
                item.BalanceAfter,
                item.Type,
                item.Status,
                item.Description,
                item.PaymentMethod,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            });
        }

        return Ok(new
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    // ============================================================
    // 6. PAYMENT STATS (ADMIN)
    // ============================================================
    /// <summary>
    /// Thống kê thanh toán (Admin)
    /// </summary>
    [HttpGet("admin/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPaymentStats()
    {
        var transactions = await _unitOfWork.Transactions
            .FindAsync(t => !t.IsDeleted && t.Status == "Success");

        var allTransactions = await _unitOfWork.Transactions
            .FindAsync(t => !t.IsDeleted);

        var totalRevenue = transactions
            .Where(t => t.Type == "Deposit")
            .Sum(t => t.Amount);

        var totalTransactions = allTransactions.Count();

        var now = DateTime.UtcNow;
        var thisMonthRevenue = transactions
            .Where(t => t.Type == "Deposit"
                && t.CreatedAt.Month == now.Month
                && t.CreatedAt.Year == now.Year)
            .Sum(t => t.Amount);

        var pendingCount = allTransactions
            .Count(t => t.Status == "Pending");

        var completedCount = allTransactions
            .Count(t => t.Status == "Success");

        var failedCount = allTransactions
            .Count(t => t.Status == "Failed");

        // 7 ngày gần nhất
        var last7Days = new List<object>();
        for (int i = 6; i >= 0; i--)
        {
            var date = now.AddDays(-i);
            var dayStart = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            var dayEnd = dayStart.AddDays(1);

            var dayRevenue = transactions
                .Where(t => t.Type == "Deposit"
                    && t.CreatedAt >= dayStart
                    && t.CreatedAt < dayEnd)
                .Sum(t => t.Amount);

            last7Days.Add(new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Revenue = dayRevenue,
                Count = allTransactions.Count(t => t.CreatedAt >= dayStart && t.CreatedAt < dayEnd)
            });
        }

        return Ok(new
        {
            TotalRevenue = totalRevenue,
            TotalTransactions = totalTransactions,
            ThisMonthRevenue = thisMonthRevenue,
            PendingCount = pendingCount,
            CompletedCount = completedCount,
            FailedCount = failedCount,
            Last7Days = last7Days
        });
    }

    // ============================================================
    // 7. GET USER TRANSACTIONS (ADMIN)
    // ============================================================
    /// <summary>
    /// Lấy giao dịch của một user (Admin)
    /// </summary>
    [HttpGet("admin/user/{userId}/transactions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserTransactions(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!Guid.TryParse(userId.ToString(), out _))
            return BadRequest(new { message = "Invalid UserId" });

        var transactions = await _unitOfWork.Transactions
            .FindAsync(t => t.UserId == userId && !t.IsDeleted);

        var totalCount = transactions.Count();
        var items = transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    // Examify.API/Controllers/PaymentController.cs

    /// <summary>
    /// Lấy tất cả giao dịch của toàn bộ user (Admin)
    /// </summary>
    [HttpGet("admin/all-transactions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] string? search = null)
    {
        var transactions = await _unitOfWork.Transactions
            .FindAsync(t => !t.IsDeleted);

        // Filter theo status
        if (!string.IsNullOrEmpty(status))
        {
            transactions = transactions.Where(t => t.Status == status);
        }

        // Filter theo type
        if (!string.IsNullOrEmpty(type))
        {
            transactions = transactions.Where(t => t.Type == type);
        }

        // Search theo description hoặc user
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            var userIds = (await _unitOfWork.Users
                .FindAsync(u => u.FullName.ToLower().Contains(searchLower) ||
                               u.Email.ToLower().Contains(searchLower)))
                .Select(u => u.Id)
                .ToList();

            transactions = transactions.Where(t =>
                userIds.Contains(t.UserId) ||
                (t.Description != null && t.Description.ToLower().Contains(searchLower))
            );
        }

        var totalCount = transactions.Count();
        var items = transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Lấy thông tin user cho từng transaction
        var result = new List<object>();
        foreach (var item in items)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(item.UserId);
            var wallet = item.WalletId.HasValue
                ? await _unitOfWork.Wallets.GetByIdAsync(item.WalletId.Value)
                : null;
            var vietnamTime = item.CreatedAt.AddHours(7);
            result.Add(new
            {
                item.Id,
                TransactionId = item.Id.ToString().Substring(0, 12).ToUpper(),
                UserId = item.UserId,
                UserName = user?.FullName ?? "Unknown",
                UserEmail = user?.Email ?? "unknown@email.com",
                item.Amount,
                item.BalanceBefore,
                item.BalanceAfter,
                item.Type,
                item.Status,
                item.Description,
                item.PaymentMethod,
                CreatedAt = vietnamTime,  
                UpdatedAt = item.UpdatedAt.HasValue ? item.UpdatedAt.Value.AddHours(7) : (DateTime?)null,
                WalletBalance = wallet?.Balance ?? 0
            });
        }

        return Ok(new
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    // ============================================================
    // 8. PAYPAL WEBHOOK (Tùy chọn - cho xử lý bất đồng bộ)
    // ============================================================
    /// <summary>
    /// Webhook nhận callback từ PayPal (không yêu cầu authentication)
    /// </summary>
    /// 
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayPalWebhook([FromBody] object payload)
    {
        // TODO: Xử lý webhook từ PayPal
        // Xác thực signature
        // Cập nhật trạng thái transaction
        // Cộng tiền vào ví

        return Ok(new { message = "Webhook received" });
    }

    // ============================================================
    // 9. APPROVE PENDING TRANSACTION (ADMIN)
    // ============================================================
    /// <summary>
    /// Duyệt giao dịch đang chờ (Admin)
    /// </summary>
    [HttpPut("admin/transaction/{transactionId}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveTransaction(Guid transactionId)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
        if (transaction is null)
            return NotFound(new { message = "Không tìm thấy giao dịch" });

        if (transaction.Status != "Pending")
            return BadRequest(new { message = "Giao dịch không ở trạng thái chờ" });

        transaction.Status = "Success";
        transaction.UpdatedAt = DateTime.UtcNow;

        // Cộng tiền vào ví nếu là Deposit
        if (transaction.Type == "Deposit")
        {
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(transaction.WalletId.Value);
            if (wallet != null)
            {
                var balanceBefore = wallet.Balance;
                wallet.Balance += transaction.Amount;
                wallet.TotalDeposited += transaction.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                transaction.BalanceBefore = balanceBefore;
                transaction.BalanceAfter = wallet.Balance;

                await _unitOfWork.Wallets.UpdateAsync(wallet);
            }
        }

        await _unitOfWork.Transactions.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Đã duyệt giao dịch thành công",
            transactionId = transaction.Id,
            status = transaction.Status
        });
    }
}