// Examify.API/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Queries.Dashboard;
using Examify.Application.DTOs.Dashboard;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy thống kê tổng quan
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetDashboardStatsQuery(userId));
        return Ok(result);
    }

    /// <summary>
    /// Lấy dữ liệu biểu đồ tiến độ (8 tuần)
    /// </summary>
    [HttpGet("progress")]
    public async Task<ActionResult<ProgressChartDto>> GetProgress([FromQuery] int weeks = 8)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetProgressChartQuery(userId, weeks));
        return Ok(result);
    }

    /// <summary>
    /// Phân tích điểm yếu và gợi ý cải thiện
    /// </summary>
    [HttpGet("weakness")]
    public async Task<ActionResult<WeaknessAnalysisDto>> GetWeaknessAnalysis()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetWeaknessAnalysisQuery(userId));
        return Ok(result);
    }
}