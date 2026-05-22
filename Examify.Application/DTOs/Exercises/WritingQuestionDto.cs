// Examify.Application/DTOs/Exercises/WritingQuestionDto.cs
namespace Examify.Application.DTOs.Exercises;

/// <summary>
/// DTO cho câu hỏi Writing
/// </summary>
public class WritingQuestionDto
{
    /// <summary>
    /// ID câu hỏi
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Loại Task (1: Task 1, 2: Task 2)
    /// </summary>
    public int TaskType { get; set; }

    /// <summary>
    /// Đề bài
    /// </summary>
    public string PromptText { get; set; } = string.Empty;

    /// <summary>
    /// URL hình ảnh mẫu (cho biểu đồ, bảng)
    /// </summary>
    public string? SampleImageUrl { get; set; }

    /// <summary>
    /// Bài mẫu tham khảo
    /// </summary>
    public string? ModelAnswer { get; set; }

    /// <summary>
    /// JSON tiêu chí chấm điểm
    /// </summary>
    public string? RubricJson { get; set; }

    /// <summary>
    /// Số từ tối thiểu
    /// </summary>
    public int MinWords { get; set; }

    /// <summary>
    /// Số từ tối đa
    /// </summary>
    public int MaxWords { get; set; }

    /// <summary>
    /// Thời gian khuyến nghị (phút)
    /// </summary>
    public int RecommendedTimeMinutes { get; set; }
}