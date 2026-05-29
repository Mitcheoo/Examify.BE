// Examify.Application/DTOs/Exercises/ExerciseDto.cs
namespace Examify.Application.DTOs.Exercises;

/// <summary>
/// DTO cho bài thi (Exercise)
/// </summary>
public class ExerciseDto
{
    /// <summary>
    /// ID của bài thi
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Loại kỹ năng (0: Reading, 1: Listening, 2: Writing, 3: Speaking)
    /// </summary>
    public int Skill { get; set; }

    /// <summary>
    /// Tên kỹ năng (Reading, Listening, Writing, Speaking)
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Tiêu đề bài thi
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả bài thi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL file audio (cho Listening)
    /// </summary>
    public string? AudioUrl { get; set; }

    /// <summary>
    /// Đoạn văn (cho Reading cũ)
    /// </summary>
    public string? Passage { get; set; }

    /// <summary>
    /// JSON chứa 3 đoạn văn (cho Reading VSTEP)
    /// </summary>
    public string? PassagesJson { get; set; }

    /// <summary>
    /// Tổng số Parts (Reading có 3 Parts)
    /// </summary>
    public int TotalParts { get; set; }

    /// <summary>
    /// Tổng số câu hỏi
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Thời gian làm bài (giây)
    /// </summary>
    public int TimeLimitSeconds { get; set; }

    /// <summary>
    /// Số lượt làm bài
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Danh sách các Part trong bài thi
    /// </summary>
    public List<PartDto> Parts { get; set; } = new();

    /// <summary>
    /// Danh sách câu hỏi Reading (nếu có)
    /// </summary>
    public List<ReadingQuestionDto> ReadingQuestions { get; set; } = new();

    /// <summary>
    /// Danh sách câu hỏi Listening (nếu có)
    /// </summary>
    public List<ListeningQuestionDto> ListeningQuestions { get; set; } = new();

    /// <summary>
    /// Danh sách câu hỏi Writing (nếu có)
    /// </summary>
    public List<WritingQuestionDto> WritingQuestions { get; set; } = new();

    /// <summary>
    /// Danh sách câu hỏi Speaking (nếu có)
    /// </summary>
    public List<SpeakingQuestionDto> SpeakingQuestions { get; set; } = new();
}