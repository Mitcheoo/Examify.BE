// Examify.API/Controllers/SessionController.cs
using Examify.Application.Cqrs.Commands.Session;
using Examify.Application.Cqrs.Queries.Session;
using Examify.Application.DTOs.FullTest;
using Examify.Application.DTOs.Session;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly IMediator _mediator;

    public SessionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lưu câu trả lời tạm (draft) - Gửi nhiều câu 1 lần
    /// </summary>
    [HttpPost("answer")]
    public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerRequestDto request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Kiểm tra session thuộc về user hiện tại
        var session = await _mediator.Send(new GetSessionByIdQuery(request.SessionId));
        if (session.UserId != userId)
            return Forbid();
        if (request.Answers == null || !request.Answers.Any())
            return BadRequest(new { message = "No answers provided" });

        // ✅ LỌC BỎ CÂU TRẢ LỜI KHÔNG HỢP LỆ
        var validAnswers = request.Answers
            .Where(a => a.QuestionId != Guid.Empty && !string.IsNullOrEmpty(a.UserAnswer))
            .ToList();

        var command = new SaveAnswerCommand(request.SessionId, request.Answers);
        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Lấy tất cả câu trả lời tạm của session
    /// </summary>
    [HttpGet("{sessionId}/answers")]
    public async Task<ActionResult<List<SessionAnswerDto>>> GetAnswers(Guid sessionId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _mediator.Send(new GetSessionByIdQuery(sessionId));
        if (session.UserId != userId)
            return Forbid();

        var query = new GetAnswersQuery(sessionId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Nộp bài và chấm điểm
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<FullTestResultResponse>> Submit([FromBody] SubmitSessionRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _mediator.Send(new GetSessionByIdQuery(request.SessionId));
        if (session.UserId != userId)
            return Forbid();

        var command = new SubmitSessionCommand(request.SessionId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    [HttpDelete("{sessionId}/answers")]
    public async Task<IActionResult> ClearDraftAnswers(Guid sessionId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _mediator.Send(new GetSessionByIdQuery(sessionId));
        if (session.UserId != userId)
            return Forbid();

        await _mediator.Send(new ClearDraftAnswersCommand(sessionId));
        return Ok(new { message = "Draft answers cleared" });
    }
}

public class SubmitSessionRequest
{
    [JsonRequired]
    public Guid SessionId { get; set; }
}