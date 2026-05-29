// Examify.API/Controllers/ReadingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Commands.Reading;
using Examify.Application.Cqrs.Queries.Exercises;
using Examify.Application.Cqrs.Queries.Submissions;
using Examify.Application.DTOs.Exercises;
using Examify.Application.DTOs.Submissions;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]  // ✅ Yêu cầu đăng nhập để dùng API
public class ReadingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReadingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách tất cả bài thi Reading
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<ExerciseDto>>> GetExercisesList()
    {
        var query = new GetExercisesListQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết đề thi Reading theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetExercise(Guid id)
    {
        var query = new GetExerciseQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Exercise not found" });

        return Ok(result);
    }

    /// <summary>
    /// Lấy đề thi Reading kèm câu hỏi (để làm bài)
    /// </summary>
    [HttpGet("exam/{id}")]
    public async Task<ActionResult<ReadingExamDto>> GetExam(Guid id)
    {
        var query = new GetReadingExamQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Exam not found" });

        return Ok(result);
    }

    /// <summary>
    /// Nộp bài thi Reading
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<SubmissionResultDto>> Submit(SubmitReadingCommand command)
    {
        // Lấy UserId từ token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        command.UserId = Guid.Parse(userIdClaim);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Xem kết quả bài thi Reading theo SubmissionId
    /// </summary>
    [HttpGet("result/{submissionId}")]
    public async Task<ActionResult<SubmissionResultDto>> GetResult(Guid submissionId)
    {
        var query = new GetSubmissionResultQuery(submissionId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Result not found" });

        // Kiểm tra quyền: chỉ chủ nhân hoặc Admin mới được xem
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (result.UserId != Guid.Parse(userIdClaim) && !isAdmin)
            return Forbid();

        return Ok(result);
    }

    /// <summary>
    /// Lấy lịch sử bài làm Reading của user hiện tại
    /// </summary>
    [HttpGet("my-submissions")]
    public async Task<ActionResult<List<MySubmissionItemDto>>> GetMySubmissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        var userId = Guid.Parse(userIdClaim);
        var query = new GetMySubmissionsQuery(userId, 0); // 0 = Reading
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}