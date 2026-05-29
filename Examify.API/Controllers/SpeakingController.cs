// Examify.API/Controllers/SpeakingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Commands.Speaking;
using Examify.Application.Cqrs.Queries.Exercises;
using Examify.Application.Cqrs.Queries.Submissions;
using Examify.Application.DTOs.Exercises;
using Examify.Application.DTOs.Submissions;
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
    /// Lấy danh sách tất cả bài thi Speaking
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<ExerciseDto>>> GetExercisesList()
    {
        var query = new GetExercisesListQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết đề thi Speaking theo ID
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
    /// Lấy đề thi Speaking kèm câu hỏi (để làm bài)
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
    /// Upload file audio cho câu hỏi Speaking
    /// </summary>
    [HttpPost("upload-audio/{questionId}")]
    public async Task<IActionResult> UploadAudio(Guid questionId, IFormFile audioFile)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        if (audioFile == null || audioFile.Length == 0)
            return BadRequest(new { message = "Audio file is required" });

        // Kiểm tra định dạng file
        var allowedExtensions = new[] { ".mp3", ".wav", ".m4a" };
        var fileExtension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(new { message = "Only MP3, WAV, M4A files are allowed" });

        // Giới hạn kích thước file (10MB)
        if (audioFile.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File size must be less than 10MB" });

        var command = new UploadAudioCommand
        {
            QuestionId = questionId,
            UserId = Guid.Parse(userIdClaim),
            AudioFile = audioFile
        };

        var result = await _mediator.Send(command);
        return Ok(new { audioUrl = result });
    }

    /// <summary>
    /// Nộp bài thi Speaking (gửi transcript hoặc audio URL)
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<SubmissionResultDto>> Submit(SubmitSpeakingCommand command)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        command.UserId = Guid.Parse(userIdClaim);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Xem kết quả bài thi Speaking theo SubmissionId
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
    /// Lấy lịch sử bài làm Speaking của user hiện tại
    /// </summary>
    [HttpGet("my-submissions")]
    public async Task<ActionResult<List<MySubmissionItemDto>>> GetMySubmissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not authenticated" });

        var userId = Guid.Parse(userIdClaim);
        var query = new GetMySubmissionsQuery(userId, 3); // 3 = Speaking
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}