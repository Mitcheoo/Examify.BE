using Examify.Application.Cqrs.Commands.Speaking;
using Examify.Application.Cqrs.Queries.Exercises;
using Examify.Application.Cqrs.Queries.Submissions;
using Examify.Application.DTOs.Exercises;
using Examify.Application.DTOs.Submissions;
using Examify.Core.Enums;
using Examify.Core.Interfaces;
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
    private readonly IAIGradingService _aiGradingService;

    public SpeakingController(IMediator mediator, IAIGradingService aiGradingService)
    {
        _mediator = mediator;
        _aiGradingService = aiGradingService;
    }

    /// <summary>
    /// Lấy danh sách bài thi Speaking
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<ExerciseDto>>> GetExercisesList()
    {
        var query = new GetExercisesListQuery(SkillType.Speaking);
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
    /// ✅ PREVIEW TRANSCRIPT - Kiểm tra chất lượng ghi âm trước khi nộp
    /// </summary>
    [HttpPost("preview-transcript")]

    public async Task<ActionResult<object>> PreviewTranscript([FromForm] IFormFile audio)
    {
        if (audio == null || audio.Length == 0)
        {
            return BadRequest(new
            {
                transcript = "",
                isValid = false,
                message = "Không có file audio"
            });
        }

        try
        {
            // Đọc file thành byte[]
            using var ms = new MemoryStream();
            await audio.CopyToAsync(ms);
            var audioData = ms.ToArray();

            // Gọi Whisper API
            var transcript = await _aiGradingService.SpeechToTextAsync(audioData);

            // Kiểm tra chất lượng transcript
            var cleanTranscript = transcript?.Replace(".", "").Replace(" ", "").Replace(",", "").Trim() ?? "";
            var isValid = !string.IsNullOrWhiteSpace(transcript) && cleanTranscript.Length >= 2;

            // Log để debug
            Console.WriteLine($"📝 Preview Transcript: '{transcript}'");
            Console.WriteLine($"📊 Clean length: {cleanTranscript.Length}, IsValid: {isValid}");

            return Ok(new
            {
                transcript = transcript ?? string.Empty,
                length = transcript?.Length ?? 0,
                cleanLength = cleanTranscript.Length,
                isValid = isValid,
                message = isValid ? "Chất lượng ghi âm tốt" : "Chất lượng ghi âm kém, vui lòng ghi âm lại"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Preview transcript error: {ex.Message}");
            return Ok(new
            {
                transcript = "",
                length = 0,
                cleanLength = 0,
                isValid = false,
                message = $"Lỗi: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Nộp bài Speaking (upload audio)
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