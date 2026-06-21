// Examify.API/Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AutoMapper;  // ✅ THÊM DÒNG NÀY
using Examify.Application.Cqrs.Commands.Admin.Exercise;
using Examify.Application.Cqrs.Commands.Admin.FullTest;
using Examify.Application.Cqrs.Commands.Admin.Question;
using Examify.Application.Cqrs.Queries.Exercises;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;  // ✅ THÊM DÒNG NÀY

    public AdminController(IMediator mediator, IUnitOfWork unitOfWork, IMapper mapper)  // ✅ SỬA CONSTRUCTOR
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _mapper = mapper;  // ✅ THÊM DÒNG NÀY
    }

    // ============================================================
    // 0. GET EXERCISES (THÊM MỚI - ĐỂ FRONTEND LẤY DANH SÁCH)
    // ============================================================

    /// <summary>
    /// Lấy danh sách Exercise (có thể lọc theo skill)
    /// </summary>
    [HttpGet("exercises")]
    public async Task<ActionResult<List<ExerciseDto>>> GetExercises([FromQuery] int? skill)
    {
        // Lấy tất cả Exercise (không phải Full Test, chưa bị xóa)
        var exercises = await _unitOfWork.Exercises
            .FindAsync(e => !e.IsDeleted && !e.IsFullTest);

        // Lọc theo skill nếu có
        if (skill.HasValue)
        {
            exercises = exercises.Where(e => e.Skill == skill.Value).ToList();
        }

        // Sắp xếp theo tên
        exercises = exercises.OrderBy(e => e.Title).ToList();

        return Ok(_mapper.Map<List<ExerciseDto>>(exercises));
    }

    // ============================================================
    // 1. EXERCISE MANAGEMENT
    // ============================================================

    [HttpPost("exercises")]
    public async Task<ActionResult<ExerciseDto>> CreateExercise([FromBody] CreateExerciseDto dto)
    {
        var result = await _mediator.Send(new CreateExerciseCommand(dto));
        return Ok(result);
    }

    [HttpPut("exercises/{id}")]
    public async Task<ActionResult<ExerciseDto>> UpdateExercise(Guid id, [FromBody] UpdateExerciseDto dto)
    {
        var result = await _mediator.Send(new UpdateExerciseCommand(id, dto));
        return Ok(result);
    }

    [HttpDelete("exercises/{id}")]
    public async Task<IActionResult> DeleteExercise(Guid id)
    {
        var exercise = await _mediator.Send(new GetExerciseByIdQuery(id));
        if (exercise == null)
            return NotFound();

        exercise.IsDeleted = true;
        exercise.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Exercises.UpdateAsync(exercise);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Exercise deleted successfully" });
    }

    // ============================================================
    // 2. FULL TEST MANAGEMENT
    // ============================================================

    [HttpPost("full-tests")]
    public async Task<ActionResult<ExerciseDto>> CreateFullTest([FromBody] CreateFullTestDto dto)
    {
        var result = await _mediator.Send(new CreateFullTestCommand(dto));
        return Ok(result);
    }

    [HttpPut("full-tests/{id}")]
    public async Task<ActionResult<ExerciseDto>> UpdateFullTest(Guid id, [FromBody] UpdateFullTestDto dto)
    {
        var result = await _mediator.Send(new UpdateFullTestCommand(id, dto));
        return Ok(result);
    }

    // ============================================================
    // 3. READING QUESTIONS
    // ============================================================

    [HttpPost("exercises/{exerciseId}/reading-questions/batch")]
    public async Task<ActionResult<List<ReadingQuestionDto>>> CreateReadingQuestions(
        Guid exerciseId,
        [FromBody] CreateReadingQuestionsDto dto)
    {
        var result = await _mediator.Send(new CreateReadingQuestionsCommand(exerciseId, dto));
        return Ok(result);
    }

    // ============================================================
    // 4. LISTENING QUESTIONS
    // ============================================================

    [HttpPost("exercises/{exerciseId}/listening-questions/batch")]
    public async Task<ActionResult<List<ListeningQuestionDto>>> CreateListeningQuestions(
        Guid exerciseId,
        [FromBody] CreateListeningQuestionsDto dto)
    {
        var result = await _mediator.Send(new CreateListeningQuestionsCommand(exerciseId, dto));
        return Ok(result);
    }

    // ============================================================
    // 5. WRITING QUESTIONS
    // ============================================================

    [HttpPost("exercises/{exerciseId}/writing-questions/batch")]
    public async Task<ActionResult<List<WritingQuestionDto>>> CreateWritingQuestions(
        Guid exerciseId,
        [FromBody] CreateWritingQuestionsDto dto)
    {
        var result = await _mediator.Send(new CreateWritingQuestionsCommand(exerciseId, dto));
        return Ok(result);
    }

    // ============================================================
    // 6. SPEAKING QUESTIONS
    // ============================================================

    [HttpPost("exercises/{exerciseId}/speaking-questions/batch")]
    public async Task<ActionResult<List<SpeakingQuestionDto>>> CreateSpeakingQuestions(
        Guid exerciseId,
        [FromBody] CreateSpeakingQuestionsDto dto)
    {
        var result = await _mediator.Send(new CreateSpeakingQuestionsCommand(exerciseId, dto));
        return Ok(result);
    }
}