// Examify.API/Controllers/SubmissionsController.cs
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
    /// Lấy chi tiết kết quả bài làm theo SubmissionId
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

        // ✅ ĐÚNG - So sánh UserId của submission với userId hiện tại
        if (result.UserId != userId && !isAdmin)
            return Forbid();

        return Ok(result);
    }
}