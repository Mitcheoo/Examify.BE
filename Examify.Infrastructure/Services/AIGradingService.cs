// Examify.Infrastructure/Services/AIGradingService.cs
using Examify.Core.Interfaces;
using Examify.Infrastructure.External;
using System.Text.Json;

namespace Examify.Infrastructure.Services;

public class AIGradingService : IAIGradingService
{
    private readonly IDeepSeekApiClient _deepSeekClient;

    public AIGradingService(IDeepSeekApiClient deepSeekClient)
    {
        _deepSeekClient = deepSeekClient;
    }

    public async Task<WritingGradeResult> GradeWritingAsync(string essay, string prompt)
    {
        var systemPrompt = @"
Bạn là giám khảo chấm thi tiếng Anh VSTEP với 10 năm kinh nghiệm.
Hãy chấm bài viết dưới đây theo thang điểm 10 với các tiêu chí:

1. Task Response (Nội dung) - 40%: Đánh giá ý tưởng, lập luận, hoàn thành yêu cầu đề bài
2. Coherence & Cohesion (Tổ chức) - 20%: Đánh giá bố cục, tính mạch lạc, liên kết ý
3. Lexical Resource (Từ vựng) - 20%: Đánh giá vốn từ, độ đa dạng và chính xác
4. Grammatical Range & Accuracy (Ngữ pháp) - 20%: Đánh giá cấu trúc câu, độ chính xác ngữ pháp

Trả về KẾT QUẢ DƯỚI DẠNG JSON với cấu trúc:
{
    ""taskResponseScore"": double (0-10),
    ""coherenceCohesionScore"": double (0-10),
    ""lexicalResourceScore"": double (0-10),
    ""grammarRangeScore"": double (0-10),
    ""totalScore"": double (0-10),
    ""strengths"": ""string (3-5 điểm mạnh)"",
    ""weaknesses"": ""string (3-5 điểm yếu)"",
    ""suggestions"": ""string (gợi ý cải thiện)""
}";

        var userPrompt = $"Đề bài: {prompt}\n\nBài viết của thí sinh:\n{essay}";

        var messages = new List<DeepSeekMessage>
        {
            new DeepSeekMessage { role = "system", content = systemPrompt },
            new DeepSeekMessage { role = "user", content = userPrompt }
        };

        var response = await _deepSeekClient.ChatCompletionAsync(messages);
        var content = response.choices.FirstOrDefault()?.message.content ?? "{}";

        var result = JsonSerializer.Deserialize<WritingGradeResult>(content);

        return result ?? new WritingGradeResult
        {
            TaskResponseScore = 0,
            CoherenceCohesionScore = 0,
            LexicalResourceScore = 0,
            GrammarRangeScore = 0,
            TotalScore = 0,
            Strengths = "Không thể chấm điểm bài viết. Vui lòng thử lại.",
            Weaknesses = "",
            Suggestions = ""
        };
    }

    public async Task<SpeakingGradeResult> GradeSpeakingAsync(string transcript, string question)
    {
        // TODO: Implement Speaking grading with DeepSeek
        return new SpeakingGradeResult
        {
            ContentScore = 7.0,
            OrganizationScore = 6.5,
            GrammarScore = 7.0,
            VocabularyScore = 7.5,
            TotalScore = 7.0,
            Strengths = "Mock response - Replace with real AI",
            Weaknesses = "Mock response",
            Suggestions = "Mock response"
        };
    }

    public async Task<string> SpeechToTextAsync(byte[] audioData)
    {
        // TODO: Implement Whisper API
        return await Task.FromResult("Sample transcript - Replace with real Whisper API");
    }
}