// Examify.Infrastructure/Services/AIGradingService.cs
using Examify.Core.Interfaces;

namespace Examify.Infrastructure.Services;

public class AIGradingService : IAIGradingService
{
    public async Task<WritingGradeResult> GradeWritingAsync(string essay, string prompt)
    {
        // TODO: Sau này tích hợp DeepSeek API thật
        return await Task.FromResult(new WritingGradeResult
        {
            TaskResponseScore = 7.5,
            CoherenceCohesionScore = 7.0,
            LexicalResourceScore = 7.5,
            GrammarRangeScore = 7.0,
            TotalScore = 7.3,
            Strengths = "Bài viết tốt, bố cục rõ ràng.",
            Weaknesses = "Còn một số lỗi ngữ pháp.",
            Suggestions = "Xem lại thì quá khứ."
        });
    }

    public async Task<SpeakingGradeResult> GradeSpeakingAsync(string transcript, string question)
    {
        return await Task.FromResult(new SpeakingGradeResult
        {
            ContentScore = 7.0,
            OrganizationScore = 6.5,
            GrammarScore = 7.0,
            VocabularyScore = 7.5,
            TotalScore = 7.0,
            Strengths = "Trả lời đúng trọng tâm.",
            Weaknesses = "Còn lúng túng.",
            Suggestions = "Luyện tập thêm."
        });
    }

    public async Task<string> SpeechToTextAsync(byte[] audioData)
    {
        // TODO: Sau này tích hợp Whisper API thật
        return await Task.FromResult("Sample transcript from audio");
    }
}