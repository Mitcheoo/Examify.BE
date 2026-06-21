using Examify.Application.Cqrs.Commands.Speaking;
using Examify.Application.Cqrs.Queries.Exercises;
using Examify.Application.Cqrs.Queries.Submissions;
using Examify.Application.DTOs.Exercises;
using Examify.Application.DTOs.Submissions;
using Examify.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SpeakingController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpeakingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách bài thi Speaking
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<ExerciseDto>>> GetExercisesList()
    {
        var query = new GetExercisesListQuery(SkillType.Speaking);  // ✅ SỬA
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    /// <summary>
    /// Lấy chi tiết đề thi Speaking
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
    /// Lấy đề thi Speaking kèm câu hỏi
    /// </summary>
    [HttpGet("exam/{id}")]
    public async Task<ActionResult<SpeakingExamDto>> GetExam(Guid id)
    {
        var query = new GetSpeakingExamQuery(id);
        var result = await _mediator.Send(query);
        if (result == null)
            return NotFound(new { message = "Exam not found" });
        return Ok(result);
    }

    /// <summary>
    /// Nộp bài Speaking (upload audio - hỗ trợ nhiều file)
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<SubmissionDetailDto>> Submit([FromForm] SubmitSpeakingCommand command)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        command.UserId = Guid.Parse(userIdClaim);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Lấy kết quả Speaking
    /// </summary>
    [HttpGet("result/{submissionId}")]
    public async Task<ActionResult<SubmissionDetailDto>> GetResult(Guid submissionId)
    {
        var query = new GetSubmissionResultQuery(submissionId);
        var result = await _mediator.Send(query);
        if (result == null)
            return NotFound(new { message = "Result not found" });

        // ✅ KIỂM TRA userIdClaim TRƯỚC KHI PARSE
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        var userId = Guid.Parse(userIdClaim);
        var isAdmin = User.IsInRole("Admin");

        if (result.Id != userId && !isAdmin)
            return Forbid();

        return Ok(result);
    }

    /// <summary>
    /// Lấy lịch sử bài làm Speaking
    /// </summary>
    [HttpGet("my-submissions")]
    public async Task<ActionResult<List<MySubmissionItemDto>>> GetMySubmissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var query = new GetMySubmissionsQuery(userId, 3);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}