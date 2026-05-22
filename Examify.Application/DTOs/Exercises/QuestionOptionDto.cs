// Examify.Application/DTOs/Exercises/QuestionOptionDto.cs
namespace Examify.Application.DTOs.Exercises;

/// <summary>
/// DTO cho một đáp án trong câu hỏi trắc nghiệm
/// </summary>
public class QuestionOptionDto
{
    /// <summary>
    /// Chữ cái đáp án (A, B, C, D)
    /// </summary>
    public string Letter { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung đáp án
    /// </summary>
    public string Text { get; set; } = string.Empty;
}