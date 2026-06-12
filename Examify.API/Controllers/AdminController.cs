// Examify.API/Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Commands.Admin;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ========== EXERCISE MANAGEMENT ==========

    /// <summary>
    /// Tạo bài thi mới
    /// </summary>
    [HttpPost("exercises")]
    public async Task<ActionResult<ExerciseDto>> CreateExercise([FromBody] CreateExerciseDto dto)
    {
        var result = await _mediator.Send(new CreateExerciseCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật bài thi
    /// </summary>
    [HttpPut("exercises/{id}")]
    public async Task<ActionResult<ExerciseDto>> UpdateExercise(Guid id, [FromBody] UpdateExerciseDto dto)
    {
        var result = await _mediator.Send(new UpdateExerciseCommand(id, dto));
        return Ok(result);
    }

    /// <summary>
    /// Xóa bài thi (soft delete)
    /// </summary>
    [HttpDelete("exercises/{id}")]
    public async Task<IActionResult> DeleteExercise(Guid id)
    {
        var result = await _mediator.Send(new DeleteExerciseCommand(id));
        return Ok(new { success = result });
    }

    // ========== PART MANAGEMENT ==========

    /// <summary>
    /// Thêm Part cho bài thi Reading
    /// </summary>
    [HttpPost("exercises/{exerciseId}/parts")]
    public async Task<ActionResult<PartDto>> CreatePart(Guid exerciseId, [FromBody] CreatePartDto dto)
    {
        var result = await _mediator.Send(new CreatePartCommand(exerciseId, dto));
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật Part
    /// </summary>
    [HttpPut("exercises/{exerciseId}/parts/{partNumber}")]
    public async Task<ActionResult<PartDto>> UpdatePart(Guid exerciseId, int partNumber, [FromBody] UpdatePartDto dto)
    {
        var result = await _mediator.Send(new UpdatePartCommand(exerciseId, partNumber, dto));
        return Ok(result);
    }

    /// <summary>
    /// Xóa Part
    /// </summary>
    [HttpDelete("exercises/{exerciseId}/parts/{partNumber}")]
    public async Task<IActionResult> DeletePart(Guid exerciseId, int partNumber)
    {
        var result = await _mediator.Send(new DeletePartCommand(exerciseId, partNumber));
        return Ok(new { success = result });
    }

    // ========== READING QUESTIONS MANAGEMENT ==========

    [HttpPost("exercises/{exerciseId}/reading-questions")]
    public async Task<ActionResult<ReadingQuestionDto>> CreateReadingQuestion(Guid exerciseId, [FromBody] CreateReadingQuestionDto dto)
    {
        var result = await _mediator.Send(new CreateReadingQuestionCommand(exerciseId, dto));
        return Ok(result);
    }

    [HttpPut("exercises/{exerciseId}/reading-questions/{questionId}")]
    public async Task<ActionResult<ReadingQuestionDto>> UpdateReadingQuestion(Guid exerciseId, Guid questionId, [FromBody] UpdateReadingQuestionDto dto)
    {
        var result = await _mediator.Send(new UpdateReadingQuestionCommand(exerciseId, questionId, dto));
        return Ok(result);
    }

    [HttpDelete("exercises/{exerciseId}/reading-questions/{questionId}")]
    public async Task<IActionResult> DeleteReadingQuestion(Guid exerciseId, Guid questionId)
    {
        var result = await _mediator.Send(new DeleteReadingQuestionCommand(exerciseId, questionId));
        return Ok(new { success = result });
    }

    // ========== LISTENING QUESTIONS MANAGEMENT ==========

    [HttpPost("exercises/{exerciseId}/listening-questions")]
    public async Task<ActionResult<ListeningQuestionDto>> CreateListeningQuestion(Guid exerciseId, [FromBody] CreateListeningQuestionDto dto)
    {
        var result = await _mediator.Send(new CreateListeningQuestionCommand(exerciseId, dto));
        return Ok(result);
    }

    [HttpPut("exercises/{exerciseId}/listening-questions/{questionId}")]
    public async Task<ActionResult<ListeningQuestionDto>> UpdateListeningQuestion(Guid exerciseId, Guid questionId, [FromBody] UpdateListeningQuestionDto dto)
    {
        var result = await _mediator.Send(new UpdateListeningQuestionCommand(exerciseId, questionId, dto));
        return Ok(result);
    }

    [HttpDelete("exercises/{exerciseId}/listening-questions/{questionId}")]
    public async Task<IActionResult> DeleteListeningQuestion(Guid exerciseId, Guid questionId)
    {
        var result = await _mediator.Send(new DeleteListeningQuestionCommand(exerciseId, questionId));
        return Ok(new { success = result });
    }

    // ========== WRITING QUESTIONS MANAGEMENT ==========

    [HttpPost("exercises/{exerciseId}/writing-questions")]
    public async Task<ActionResult<WritingQuestionDto>> CreateWritingQuestion(Guid exerciseId, [FromBody] CreateWritingQuestionDto dto)
    {
        var result = await _mediator.Send(new CreateWritingQuestionCommand(exerciseId, dto));
        return Ok(result);
    }

    [HttpPut("exercises/{exerciseId}/writing-questions/{questionId}")]
    public async Task<ActionResult<WritingQuestionDto>> UpdateWritingQuestion(Guid exerciseId, Guid questionId, [FromBody] UpdateWritingQuestionDto dto)
    {
        var result = await _mediator.Send(new UpdateWritingQuestionCommand(exerciseId, questionId, dto));
        return Ok(result);
    }

    [HttpDelete("exercises/{exerciseId}/writing-questions/{questionId}")]
    public async Task<IActionResult> DeleteWritingQuestion(Guid exerciseId, Guid questionId)
    {
        var result = await _mediator.Send(new DeleteWritingQuestionCommand(exerciseId, questionId));
        return Ok(new { success = result });
    }

    // ========== SPEAKING QUESTIONS MANAGEMENT ==========

    [HttpPost("exercises/{exerciseId}/speaking-questions")]
    public async Task<ActionResult<SpeakingQuestionDto>> CreateSpeakingQuestion(Guid exerciseId, [FromBody] CreateSpeakingQuestionDto dto)
    {
        var result = await _mediator.Send(new CreateSpeakingQuestionCommand(exerciseId, dto));
        return Ok(result);
    }

    [HttpPut("exercises/{exerciseId}/speaking-questions/{questionId}")]
    public async Task<ActionResult<SpeakingQuestionDto>> UpdateSpeakingQuestion(Guid exerciseId, Guid questionId, [FromBody] UpdateSpeakingQuestionDto dto)
    {
        var result = await _mediator.Send(new UpdateSpeakingQuestionCommand(exerciseId, questionId, dto));
        return Ok(result);
    }

    [HttpDelete("exercises/{exerciseId}/speaking-questions/{questionId}")]
    public async Task<IActionResult> DeleteSpeakingQuestion(Guid exerciseId, Guid questionId)
    {
        var result = await _mediator.Send(new DeleteSpeakingQuestionCommand(exerciseId, questionId));
        return Ok(new { success = result });
    }
}