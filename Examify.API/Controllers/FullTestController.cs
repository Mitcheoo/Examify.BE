// Examify.API/Controllers/FullTestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Commands.FullTest;
using Examify.Application.DTOs.FullTest;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FullTestController : ControllerBase
{
    private readonly IMediator _mediator;

    public FullTestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Bắt đầu bài thi thử full test (4 kỹ năng)
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<StartFullTestResponse>> Start()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new StartFullTestCommand(userId));
        return Ok(result);
    }

    /// <summary>
    /// Lưu kết quả từng phần (Reading, Listening, Writing, Speaking)
    /// </summary>
    [HttpPost("save-part")]
    public async Task<IActionResult> SavePart([FromBody] SavePartRequest request)
    {
        var command = new SavePartCommand(
            request.SessionId,
            request.PartNumber,
            request.Answers,
            request.EssayText,
            request.AudioUrl,
            request.TimeSpentSeconds
        );

        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Nộp toàn bộ bài thi và nhận kết quả
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<FullTestResultResponse>> Submit([FromBody] SubmitFullTestCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}