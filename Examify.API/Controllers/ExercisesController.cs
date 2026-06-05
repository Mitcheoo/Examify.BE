// Examify.API/Controllers/ExercisesController.cs
using Examify.Application.Cqrs.Queries.Exercises;
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Exercises;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExercisesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách tất cả bài thi (có phân trang và lọc theo kỹ năng)
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<PagedResult<ExerciseDto>>> GetExercisesList(
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] int? skill = null,
     [FromQuery] string? search = null)
    {
        // ✅ Dùng Query MỚI (GetPagedExercisesListQuery)
        var query = new GetPagedExercisesListQuery(page, pageSize, skill, search);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết bài thi theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDetailDto>> GetExercise(Guid id)
    {
        var query = new GetExerciseDetailQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Exercise not found" });

        return Ok(result);
    }

    /// <summary>
    /// Lấy kết quả tổng hợp của bài thi (điểm cao nhất, trung bình, số lần làm)
    /// </summary>
    [HttpGet("{id}/result")]
    public async Task<ActionResult<ExerciseResultSummaryDto>> GetExerciseResult(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetExerciseResultSummaryQuery(id, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Kiểm tra người dùng đã mua bài thi này chưa
    /// </summary>
    [HttpGet("{id}/purchased")]
    public async Task<ActionResult<PurchasedStatusDto>> CheckPurchased(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new CheckExercisePurchasedQuery(id, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy tiến độ học tập của bài thi (số lần làm, điểm cao nhất, tiến bộ)
    /// </summary>
    [HttpGet("{id}/progress")]
    public async Task<ActionResult<ExerciseProgressDto>> GetProgress(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetExerciseProgressQuery(id, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}