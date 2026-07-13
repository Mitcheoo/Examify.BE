/*Examify.API / Controllers / SubmissionsController.cs*/
using Examify.Application.Cqrs.Queries.Submissions;
using Examify.Application.DTOs.Submissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubmissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// ✅ LẤY DANH SÁCH BÀI LÀM CỦA USER
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<MySubmissionItemDto>>> GetMySubmissions(
        [FromQuery] int? skill = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? offset = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        var userId = Guid.Parse(userIdClaim);

        var query = new GetMySubmissionsQuery(
            UserId: userId,
            SkillType: skill,
            Limit: limit ?? 50,
            Offset: offset ?? 0
        );

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// ✅ LẤY CHI TIẾT 1 BÀI LÀM
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SubmissionDetailDto>> GetSubmission(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        var userId = Guid.Parse(userIdClaim);
        var isAdmin = User.IsInRole("Admin");

        var query = new GetSubmissionResultQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Submission not found" });

        if (result.UserId != userId && !isAdmin)
            return Forbid();

        return Ok(result);
    }
}