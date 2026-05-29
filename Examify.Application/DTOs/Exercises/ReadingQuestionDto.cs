// Examify.Application/DTOs/Exercises/ReadingQuestionDto.cs
namespace Examify.Application.DTOs.Exercises;

/// <summary>
/// DTO cho câu hỏi Reading
/// </summary>
public class ReadingQuestionDto
{
    /// <summary>
    /// ID câu hỏi
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Số thứ tự câu hỏi
    /// </summary>
    public int OrderNumber { get; set; }

    /// <summary>
    /// Nội dung câu hỏi
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Loại câu hỏi (multiple_choice, true_false, matching)
    /// </summary>
    public string QuestionType { get; set; } = "multiple_choice";

    /// <summary>
    /// JSON chứa các đáp án A, B, C, D
    /// </summary>
    public string OptionsJson { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách đáp án đã parse (dùng cho View)
    /// </summary>
    public List<QuestionOptionDto> Options { get; set; } = new();

    /// <summary>
    /// Đáp án đúng
    /// </summary>
    public string CorrectAnswer { get; set; } = string.Empty;

    /// <summary>
    /// Giải thích đáp án
    /// </summary>
    public string? Explanation { get; set; }
}