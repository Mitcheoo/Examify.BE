// Examify.Application/DTOs/Exercises/ListeningQuestionDto.cs
namespace Examify.Application.DTOs.Exercises;

/// <summary>
/// DTO cho câu hỏi Listening
/// </summary>
public class ListeningQuestionDto
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
    /// Đáp án A
    /// </summary>
    public string OptionA { get; set; } = string.Empty;

    /// <summary>
    /// Đáp án B
    /// </summary>
    public string OptionB { get; set; } = string.Empty;

    /// <summary>
    /// Đáp án C
    /// </summary>
    public string OptionC { get; set; } = string.Empty;

    /// <summary>
    /// Đáp án D
    /// </summary>
    public string OptionD { get; set; } = string.Empty;

    /// <summary>
    /// Đáp án đúng
    /// </summary>
    public string CorrectAnswer { get; set; } = string.Empty;

    /// <summary>
    /// URL file audio (cho Part 1)
    /// </summary>
    public string? AudioUrl { get; set; }

    /// <summary>
    /// Giải thích đáp án
    /// </summary>
    public string? Explanation { get; set; }
}