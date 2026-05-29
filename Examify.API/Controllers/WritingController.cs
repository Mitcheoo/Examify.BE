using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Commands.Writing;
using Examify.Application.Cqrs.Queries.Exercises;
using Examify.Application.Cqrs.Queries.Submissions;
using Examify.Application.DTOs.Exercises;
using Examify.Application.DTOs.Submissions;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WritingController : ControllerBase
{
    private readonly IMediator _mediator;

    public WritingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách tất cả bài thi Writing
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<ExerciseDto>>> GetExercisesList()
    {
        var query = new GetExercisesListQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết đề thi Writing theo ID
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
    /// Lấy đề thi Writing kèm câu hỏi (để làm bài)
    /// </summary>
    [HttpGet("exam/{id}")]
    public async Task<ActionResult<WritingExamDto>> GetExam(Guid id)
    {
        var query = new GetWritingExamQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Exam not found" });

        return Ok(result);
    }

    /// <summary>
    /// Nộp bài thi Writing (gọi AI chấm điểm)
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<SubmissionResultDto>> Submit(SubmitWritingCommand command)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        command.UserId = Guid.Parse(userIdClaim);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Xem kết quả bài thi Writing theo SubmissionId
    /// </summary>
    [HttpGet("result/{submissionId}")]
    public async Task<ActionResult<SubmissionResultDto>> GetResult(Guid submissionId)
    {
        var query = new GetSubmissionResultQuery(submissionId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Result not found" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (result.UserId != Guid.Parse(userIdClaim) && !isAdmin)
            return Forbid();

        return Ok(result);
    }

    /// <summary>
    /// Lấy lịch sử bài làm Writing của user hiện tại
    /// </summary>
    [HttpGet("my-submissions")]
    public async Task<ActionResult<List<MySubmissionItemDto>>> GetMySubmissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        var userId = Guid.Parse(userIdClaim);
        var query = new GetMySubmissionsQuery(userId, 2); // 2 = Writing
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}