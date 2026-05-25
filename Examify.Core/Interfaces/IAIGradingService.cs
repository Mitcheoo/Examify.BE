// Examify.Core/Interfaces/IAIGradingService.cs
namespace Examify.Core.Interfaces;

public interface IAIGradingService
{
    // Chấm điểm Writing
    Task<WritingGradeResult> GradeWritingAsync(string essay, string prompt);

    // Chấm điểm Speaking
    Task<SpeakingGradeResult> GradeSpeakingAsync(string transcript, string question);

    // Speech to Text
    Task<string> SpeechToTextAsync(byte[] audioData);
}

public class WritingGradeResult
{
    public double TaskResponseScore { get; set; }
    public double CoherenceCohesionScore { get; set; }
    public double LexicalResourceScore { get; set; }
    public double GrammarRangeScore { get; set; }
    public double TotalScore { get; set; }
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string Suggestions { get; set; } = string.Empty;
}

public class SpeakingGradeResult
{
    public double ContentScore { get; set; }
    public double OrganizationScore { get; set; }
    public double GrammarScore { get; set; }
    public double VocabularyScore { get; set; }
    public double TotalScore { get; set; }
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string Suggestions { get; set; } = string.Empty;
}