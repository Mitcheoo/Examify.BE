// 📁 Examify.API/Controllers/FullTestController.cs

using Examify.Application.Cqrs.Commands.FullTest;
using Examify.Application.Cqrs.Queries.FullTest;
using Examify.Application.DTOs.FullTest;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
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
    private readonly IUnitOfWork _unitOfWork;

    public FullTestController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    // ============================================================
    // API 1: BẮT ĐẦU FULL TEST
    // ============================================================
    [HttpPost("start")]
    public async Task<ActionResult<StartFullTestResponse>> Start([FromBody] StartFullTestRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new StartFullTestCommand(userId, request.FullTestId));
        return Ok(result);
    }

    // ============================================================
    // API 2: LẤY TRẠNG THÁI
    // ============================================================
    [HttpGet("{id}/status")]
    public async Task<ActionResult<FullTestStatusDto>> GetFullTestStatus(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetFullTestStatusQuery(id, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ============================================================
    // API 3: LẤY KẾT QUẢ
    // ============================================================
    [HttpGet("{id}/result")]
    public async Task<ActionResult<FullTestResultDetailDto>> GetFullTestResult(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetFullTestResultQuery(id, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ============================================================
    // API 4: NỘP BÀI
    // ============================================================
    [HttpPost("submit")]
    public async Task<ActionResult<FullTestResultResponse>> Submit([FromBody] SubmitFullTestRequest request)
    {
        var command = new SubmitFullTestCommand(request.SessionId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ============================================================
    // API 5: ĐÓNG SESSION + TĂNG ATTEMPT COUNT ⭐
    // ============================================================
    [HttpPost("{id}/close-session")]
    public async Task<IActionResult> CloseSession(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Tìm session active
        var sessions = await _unitOfWork.FullTestSessions
            .FindAsync(s => s.UserId == userId && s.FullTestId == id && s.Status == 0);

        var session = sessions.FirstOrDefault();
        if (session is null)
            return NotFound(new { message = "No active session found" });

        // Đóng session
        session.Status = 1; // Completed
        session.EndTime = DateTime.UtcNow;
        await _unitOfWork.FullTestSessions.UpdateAsync(session);

        // Tăng AttemptCount
        var fullTest = await _unitOfWork.Exercises.GetByIdAsync(id);
        if (fullTest != null)
        {
            fullTest.AttemptCount += 1;
            await _unitOfWork.Exercises.UpdateAsync(fullTest);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Session closed successfully",
            sessionId = session.Id,
            fullTestId = id,
            attemptCount = fullTest?.AttemptCount ?? 0
        });
    }

    // ============================================================
    // API 6: TẠO SESSION MỚI ⭐
    // ============================================================
    [HttpPost("{id}/new-session")]
    public async Task<IActionResult> CreateNewSession(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Kiểm tra Full Test tồn tại
        var fullTest = await _unitOfWork.Exercises.GetByIdAsync(id);
        if (fullTest is null || !fullTest.IsFullTest)
            return NotFound(new { message = "Full Test not found" });

        // Tạo session mới
        var newSession = new FullTestSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullTestId = id,  // ✅ ĐÃ CÓ PROPERTY NÀY
            StartTime = DateTime.UtcNow,
            Status = 0,
            CurrentPart = 1,
            AttemptCounted = false,
            ReadingTimeSpent = 0,
            ListeningTimeSpent = 0,
            WritingTimeSpent = 0,
            SpeakingTimeSpent = 0
        };

        // Lấy các exercise con
        var childExercises = await _unitOfWork.Exercises
            .FindAsync(e => e.FullTestId == id && !e.IsDeleted);

        foreach (var exercise in childExercises)
        {
            int skillValue = (int)exercise.Skill;

            switch (skillValue)
            {
                case 0: // Reading
                    newSession.ReadingExerciseId = exercise.Id;
                    break;
                case 1: // Listening
                    newSession.ListeningExerciseId = exercise.Id;
                    break;
                case 2: // Writing
                    newSession.WritingExerciseId = exercise.Id;
                    break;
                case 3: // Speaking
                    newSession.SpeakingExerciseId = exercise.Id;
                    break;
            }
        }

        await _unitOfWork.FullTestSessions.AddAsync(newSession);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "New session created successfully",
            sessionId = newSession.Id,
            fullTestId = id,
            status = "InProgress",
            startTime = newSession.StartTime
        });
    }

    // ============================================================
    // API 7: KIỂM TRA SESSION ACTIVE ⭐
    // ============================================================
    [HttpGet("{id}/session/active")]
    public async Task<IActionResult> GetActiveSession(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var sessions = await _unitOfWork.FullTestSessions
            .FindAsync(s => s.UserId == userId && s.FullTestId == id && s.Status == 0);

        var session = sessions.OrderByDescending(s => s.StartTime).FirstOrDefault();

        if (session is null)
        {
            return Ok(new
            {
                hasActiveSession = false,
                message = "No active session found"
            });
        }

        return Ok(new
        {
            hasActiveSession = true,
            sessionId = session.Id,
            startTime = session.StartTime,
            currentPart = session.CurrentPart,
            status = "InProgress"
        });
    }

    // ============================================================
    // API 8: XÓA SESSION ⭐
    // ============================================================
    [HttpDelete("{id}/session/{sessionId}")]
    public async Task<IActionResult> DeleteSession(Guid id, Guid sessionId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var session = await _unitOfWork.FullTestSessions.GetByIdAsync(sessionId);
        if (session is null)
            return NotFound(new { message = "Session not found" });

        if (session.UserId != userId)
            return Forbid();

        if (session.Status == 1)
            return BadRequest(new { message = "Cannot delete a completed session" });

        session.IsDeleted = true;
        await _unitOfWork.FullTestSessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Session deleted successfully",
            sessionId = sessionId
        });
    }
}