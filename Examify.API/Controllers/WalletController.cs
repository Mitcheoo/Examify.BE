// Examify.API/Controllers/WalletController.cs
using Examify.Application.Cqrs.Queries.Payment;
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Payment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IMediator _mediator;

    public WalletController(IMediator mediator)
    {
        _mediator = mediator;
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
    // 1. LẤY THÔNG TIN VÍ
    // ============================================================
    /// <summary>
    /// Lấy thông tin ví của user hiện tại
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<WalletDto>> GetWallet()
    {
        var userId = GetUserId();
        var query = new GetWalletQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ============================================================
    // 2. LẤY SỐ DƯ
    // ============================================================
    /// <summary>
    /// Lấy số dư hiện tại của ví
    /// </summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetUserId();
        var query = new GetWalletQuery(userId);
        var result = await _mediator.Send(query);

        return Ok(new
        {
            balance = result.Balance,
            totalDeposited = result.TotalDeposited,
            totalSpent = result.TotalSpent
        });
    }

    // ============================================================
    // 3. LẤY LỊCH SỬ GIAO DỊCH
    // ============================================================
    /// <summary>
    /// Lấy lịch sử giao dịch của user
    /// </summary>
    [HttpGet("transactions")]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null)
    {
        var userId = GetUserId();
        var query = new GetTransactionsQuery(userId, page, pageSize, type);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ============================================================
    // 4. KIỂM TRA BÀI THI ĐÃ MUA CHƯA
    // ============================================================
    /// <summary>
    /// Kiểm tra user đã mua bài thi này chưa
    /// </summary>
    [HttpGet("purchased/{exerciseId}")]
    public async Task<IActionResult> CheckPurchased(Guid exerciseId)
    {
        var userId = GetUserId();
        var query = new GetPurchasedExercisesQuery(userId);
        var purchasedList = await _mediator.Send(query);

        var purchased = purchasedList.FirstOrDefault(p => p.ExerciseId == exerciseId);

        return Ok(new
        {
            IsPurchased = purchased is not null,
            PurchasedAt = purchased?.PurchasedAt,
            PaidAmount = purchased?.PaidAmount
        });
    }

    // ============================================================
    // 5. LẤY DANH SÁCH BÀI THI ĐÃ MUA
    // ============================================================
    /// <summary>
    /// Lấy danh sách tất cả bài thi user đã mua
    /// </summary>
    [HttpGet("purchased")]
    public async Task<ActionResult<List<PurchasedExerciseDto>>> GetPurchasedExercises()
    {
        var userId = GetUserId();
        var query = new GetPurchasedExercisesQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}