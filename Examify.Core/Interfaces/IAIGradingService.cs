// Examify.Core/Interfaces/IAIGradingService.cs
namespace Examify.Core.Interfaces;

public interface IAIGradingService
{
    /// <summary>
    /// Chấm điểm bài viết Writing sử dụng AI
    /// </summary>
    Task<WritingGradeResult> GradeWritingAsync(string essay, string prompt);

    /// <summary>
    /// Chấm điểm bài nói Speaking sử dụng AI
    /// </summary>
    Task<SpeakingGradeResult> GradeSpeakingAsync(string transcript, string question);

    /// <summary>
    /// Chuyển giọng nói thành văn bản (Speech-to-Text)
    /// </summary>
    Task<string> SpeechToTextAsync(byte[] audioData);
}

/// <summary>
/// Kết quả chấm điểm Writing
/// </summary>
public class WritingGradeResult
{
    public double TaskResponseScore { get; set; }      // Nội dung (40%)
    public double CoherenceCohesionScore { get; set; } // Tổ chức (20%)
    public double LexicalResourceScore { get; set; }   // Từ vựng (20%)
    public double GrammarRangeScore { get; set; }      // Ngữ pháp (20%)
    public double TotalScore { get; set; }             // Tổng điểm (thang 10)
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string Suggestions { get; set; } = string.Empty;
}

/// <summary>
/// Kết quả chấm điểm Speaking
/// </summary>
public class SpeakingGradeResult
{
    public double ContentScore { get; set; }       // Nội dung (40%)
    public double OrganizationScore { get; set; }  // Tổ chức (30%)
    public double GrammarScore { get; set; }       // Ngữ pháp (15%)
    public double VocabularyScore { get; set; }    // Từ vựng (15%)
    public double TotalScore { get; set; }         // Tổng điểm (thang 10)
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string Suggestions { get; set; } = string.Empty;
}