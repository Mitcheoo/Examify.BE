// Examify.API/Controllers/Admin/AdminSubmissionsController.cs
using Examify.Application.Cqrs.Queries.Admin.Submissions;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Examify.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminSubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminSubmissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 📊 Lấy danh sách tất cả bài nộp (Admin)
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số lượng mỗi trang (mặc định: 20)</param>
    /// <param name="search">Tìm kiếm theo tên user, email, tên bài thi</param>
    /// <param name="skill">Lọc theo kỹ năng: Reading, Listening, Writing, Speaking, FullTest</param>
    /// <param name="status">Lọc theo trạng thái: Passed, Failed, InProgress</param>
    /// <param name="fromDate">Từ ngày (yyyy-MM-dd)</param>
    /// <param name="toDate">Đến ngày (yyyy-MM-dd)</param>
    /// <param name="sortBy">Sắp xếp theo: SubmittedAt, Score, UserName</param>
    /// <param name="sortDescending">Sắp xếp giảm dần (mặc định: true)</param>
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminSubmissionDto>>> GetSubmissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? skill = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = "SubmittedAt",
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetAdminSubmissionsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Skill = skill,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 📈 Lấy thống kê bài nộp (Admin)
    /// </summary>
    /// <param name="days">Số ngày thống kê (mặc định: 30)</param>
    [HttpGet("stats")]
    public async Task<ActionResult<AdminSubmissionStatsDto>> GetStats([FromQuery] int days = 30)
    {
        var query = new GetAdminSubmissionStatsQuery { Days = days };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 📄 Lấy chi tiết bài nộp theo ID (Admin)
    /// </summary>
    /// <param name="id">ID của bài nộp</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<AdminSubmissionDto>> GetSubmission(Guid id)
    {
        var query = new GetAdminSubmissionDetailQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 📊 Lấy danh sách bài nộp của một user cụ thể (Admin)
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số lượng mỗi trang (mặc định: 20)</param>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<PagedResult<AdminSubmissionDto>>> GetUserSubmissions(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAdminSubmissionsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = userId.ToString(),
            SortBy = "SubmittedAt",
            SortDescending = true
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 📊 Lấy danh sách bài nộp theo bài thi (Admin)
    /// </summary>
    /// <param name="exerciseId">ID của bài thi</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số lượng mỗi trang (mặc định: 20)</param>
    [HttpGet("exercise/{exerciseId}")]
    public async Task<ActionResult<PagedResult<AdminSubmissionDto>>> GetExerciseSubmissions(
        Guid exerciseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAdminSubmissionsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = exerciseId.ToString(),
            SortBy = "SubmittedAt",
            SortDescending = true
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 📊 Xuất danh sách bài nộp ra Excel (Admin)
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportSubmissions(
        [FromQuery] string? search = null,
        [FromQuery] string? skill = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetAdminSubmissionsQuery
        {
            Page = 1,
            PageSize = 10000, // Lấy tất cả
            Search = search,
            Skill = skill,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            SortBy = "SubmittedAt",
            SortDescending = true
        };

        var result = await _mediator.Send(query);

        // TODO: Tạo file Excel từ result.Items
        // return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "submissions.xlsx");

        return Ok(new
        {
            message = "Xuất dữ liệu thành công",
            total = result.TotalCount,
            data = result.Items
        });
    }
}