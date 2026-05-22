// Examify.Application/DTOs/Exercises/SpeakingQuestionDto.cs
namespace Examify.Application.DTOs.Exercises;

/// <summary>
/// DTO cho câu hỏi Speaking
/// </summary>
public class SpeakingQuestionDto
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
    /// URL file audio mẫu
    /// </summary>
    public string? AudioUrl { get; set; }

    /// <summary>
    /// Thời gian chuẩn bị (giây)
    /// </summary>
    public int PreparationTime { get; set; }

    /// <summary>
    /// Thời gian nói (giây)
    /// </summary>
    public int SpeakingTime { get; set; }

    /// <summary>
    /// Câu trả lời mẫu
    /// </summary>
    public string? SampleAnswer { get; set; }
}