// Examify.Core/Interfaces/IAIGradingService.cs
using System.Text.Json.Serialization;


namespace Examify.Core.Interfaces;

public interface IAIGradingService
{
    // ============================================================
    // WRITING
    // ============================================================

    /// <summary>
    /// Chấm 1 bài viết Writing
    /// </summary>
    Task<WritingGradeResult> GradeWritingAsync(string essay, string prompt);

    /// <summary>
    /// Chấm nhiều bài viết Writing cùng lúc (Batch)
    /// </summary>
    Task<List<WritingGradeResult>> GradeWritingBatchAsync(
        List<(string essay, string prompt)> essays);

    // ============================================================
    // SPEAKING
    // ============================================================

    Task<SpeakingGradeResult> GradeSpeakingAsync(byte[] audioData, string question);
    Task<SpeakingGradeResult> GradeSpeakingContentAsync(string transcript, string question);
    Task<string> SpeechToTextAsync(byte[] audioData);
}

// ============================================================
// WRITING DETAILED FEEDBACK (✅ THÊM MỚI)
// ============================================================

public class WritingDetailedFeedback
{
    [JsonPropertyName("issue")]
    public string Issue { get; set; } = string.Empty;

    [JsonPropertyName("sentence")]
    public string Sentence { get; set; } = string.Empty;

    [JsonPropertyName("suggestion")]
    public string Suggestion { get; set; } = string.Empty;
}

// ============================================================
// SPEAKING ERROR ANALYSIS (✅ THÊM MỚI)
// ============================================================

public class SpeakingErrorAnalysis
{
    [JsonPropertyName("transcript")]
    public string Transcript { get; set; } = string.Empty;

    [JsonPropertyName("issue")]
    public string Issue { get; set; } = string.Empty;

    [JsonPropertyName("correction")]
    public string Correction { get; set; } = string.Empty;
}

// ============================================================
// WRITING GRADE RESULT (✅ CẬP NHẬT - THÊM DETAILEDFEEDBACK)
// ============================================================

public class WritingGradeResult
{
    [JsonPropertyName("taskResponseScore")]
    public double TaskResponseScore { get; set; }

    [JsonPropertyName("coherenceCohesionScore")]
    public double CoherenceCohesionScore { get; set; }

    [JsonPropertyName("lexicalResourceScore")]
    public double LexicalResourceScore { get; set; }

    [JsonPropertyName("grammarRangeScore")]
    public double GrammarRangeScore { get; set; }

    [JsonPropertyName("totalScore")]
    public double TotalScore { get; set; }

    [JsonPropertyName("strengths")]
    public string Strengths { get; set; } = string.Empty;

    [JsonPropertyName("weaknesses")]
    public string Weaknesses { get; set; } = string.Empty;

    [JsonPropertyName("suggestions")]
    public string Suggestions { get; set; } = string.Empty;

    // ✅ THÊM MỚI: Detailed Feedback
    [JsonPropertyName("detailedFeedback")]
    public List<WritingDetailedFeedback> DetailedFeedback { get; set; } = new();
}

// ============================================================
// SPEAKING GRADE RESULT (✅ CẬP NHẬT - THÊM ERRORANALYSIS)
// ============================================================

public class SpeakingGradeResult
{
    [JsonPropertyName("contentScore")]
    public double ContentScore { get; set; }

    [JsonPropertyName("organizationScore")]
    public double OrganizationScore { get; set; }

    [JsonPropertyName("grammarScore")]
    public double GrammarScore { get; set; }

    [JsonPropertyName("vocabularyScore")]
    public double VocabularyScore { get; set; }

    [JsonPropertyName("totalScore")]
    public double TotalScore { get; set; }

    [JsonPropertyName("strengths")]
    public string Strengths { get; set; } = string.Empty;

    [JsonPropertyName("weaknesses")]
    public string Weaknesses { get; set; } = string.Empty;

    [JsonPropertyName("suggestions")]
    public string Suggestions { get; set; } = string.Empty;

    // ✅ THÊM MỚI: Error Analysis
    [JsonPropertyName("errorAnalysis")]
    public List<SpeakingErrorAnalysis> ErrorAnalysis { get; set; } = new();
}

// ============================================================
// BATCH WRITING RESPONSE (CHO OPENAI)
// ============================================================

public class BatchWritingResponse
{
    [JsonPropertyName("results")]
    public List<WritingGradeResult> Results { get; set; } = new();
}