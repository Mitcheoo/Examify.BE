// Examify.Application/DTOs/Exercises/PartDto.cs
namespace Examify.Application.DTOs.Exercises;

/// <summary>
/// DTO cho Part trong bài thi
/// </summary>
public class PartDto
{
    /// <summary>
    /// ID của Part
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Số thứ tự Part (1, 2, 3)
    /// </summary>
    public int PartNumber { get; set; }

    /// <summary>
    /// Tiêu đề Part
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Đoạn văn (cho Reading)
    /// </summary>
    public string? Passage { get; set; }

    /// <summary>
    /// URL file audio (cho Listening Part 2,3)
    /// </summary>
    public string? AudioUrl { get; set; }

    /// <summary>
    /// Danh sách câu hỏi trong Part
    /// </summary>
    public List<ReadingQuestionDto> Questions { get; set; } = new();
}