// Examify.API/Controllers/VocabularyController.cs
using Examify.Application.Cqrs.Commands.Vocabulary;
using Examify.Application.Cqrs.Queries.Vocabulary;
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Vocabulary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VocabularyController : ControllerBase
{
    private readonly IMediator _mediator;

    public VocabularyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated");

        return Guid.Parse(userIdClaim);
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    // ============================================================
    // USER APIs
    // ============================================================

    /// <summary>
    /// Lấy danh sách từ vựng (có filter và phân trang)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<VocabularyWordDto>>> GetVocabularyList(
        [FromQuery] string? search = null,
        [FromQuery] string? level = null,
        [FromQuery] string? topic = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var userId = GetUserId();
        var filter = new VocabularyFilterDto
        {
            Search = search,
            Level = level,
            Topic = topic,
            Status = status,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var query = new GetVocabularyListQuery(filter, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết từ vựng
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VocabularyWordDto>> GetVocabularyDetail(Guid id)
    {
        var userId = GetUserId();
        var query = new GetVocabularyDetailQuery(id, userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Đánh dấu đã thuộc/chưa thuộc từ vựng
    /// </summary>
    [HttpPatch("{id}/mastered")]
    public async Task<ActionResult<VocabularyProgressDto>> ToggleMastered(Guid id, [FromBody] bool isMastered)
    {
        var userId = GetUserId();
        var command = new ToggleMasteredCommand(id, userId, isMastered);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thống kê học từ vựng
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<VocabularyStatsDto>> GetStats([FromQuery] int days = 30)
    {
        var userId = GetUserId();
        var query = new GetVocabularyStatsQuery(userId, days);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ============================================================
    // ADMIN APIs (Full CRUD)
    // ============================================================

    /// <summary>
    /// Tạo từ vựng mới (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VocabularyWordDto>> CreateWord([FromBody] CreateVocabularyWordDto dto)
    {
        var command = new CreateVocabularyWordCommand(dto);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetVocabularyDetail), new { id = result.Id }, result);
    }

    /// <summary>
    /// Cập nhật từ vựng (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VocabularyWordDto>> UpdateWord(Guid id, [FromBody] UpdateVocabularyWordDto dto)
    {
        var command = new UpdateVocabularyWordCommand(id, dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Xóa từ vựng (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> DeleteWord(Guid id)
    {
        var command = new DeleteVocabularyWordCommand(id);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Import nhiều từ vựng (Admin only)
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ImportVocabularyResultDto>> ImportWords([FromBody] List<CreateVocabularyWordDto> words)
    {
        if (words == null || !words.Any())
        {
            return BadRequest(new { message = "Danh sách từ vựng không được để trống" });
        }

        var successCount = 0;
        var failedWords = new List<string>();

        foreach (var dto in words)
        {
            try
            {
                var command = new CreateVocabularyWordCommand(dto);
                await _mediator.Send(command);
                successCount++;
            }
            catch (Exception ex)
            {
                failedWords.Add($"{dto.Word}: {ex.Message}");
            }
        }

        return Ok(new ImportVocabularyResultDto
        {
            Total = words.Count,
            SuccessCount = successCount,
            FailedCount = words.Count - successCount,
            FailedWords = failedWords
        });
    }
}

public class ImportVocabularyResultDto
{
    public int Total { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> FailedWords { get; set; } = new();
}