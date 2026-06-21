// Examify.API/Controllers/FullTestController.cs
using Examify.Application.Cqrs.Commands.FullTest;
using Examify.Application.Cqrs.Queries.FullTest;
using Examify.Application.DTOs.FullTest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FullTestController : ControllerBase
{
    private readonly IMediator _mediator;

    public FullTestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartFullTestResponse>> Start()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new StartFullTestCommand(userId));
        return Ok(result);
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<FullTestStatusDto>> GetFullTestStatus(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetFullTestStatusQuery(id, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/result")]
    public async Task<ActionResult<FullTestResultDetailDto>> GetFullTestResult(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetFullTestResultQuery(id, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("submit")]
    public async Task<ActionResult<FullTestResultResponse>> Submit([FromBody] SubmitFullTestRequest request)
    {
        var command = new SubmitFullTestCommand(request.SessionId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}