// Examify.Infrastructure/Services/AIGradingService.cs
using Examify.Core.Interfaces;
using Examify.Infrastructure.External;
using System.Text.Json;

namespace Examify.Infrastructure.Services;

public class AIGradingService : IAIGradingService
{
    private readonly IOpenAIClient _openAIClient;
    private readonly IWhisperApiClient _whisperClient;

    public AIGradingService(IOpenAIClient openAIClient, IWhisperApiClient whisperClient)
    {
        _openAIClient = openAIClient;
        _whisperClient = whisperClient;
    }

    // ============================================================
    // WRITING - SINGLE
    // ============================================================

    public async Task<WritingGradeResult> GradeWritingAsync(string essay, string prompt)
    {
        if (string.IsNullOrWhiteSpace(essay))
        {
            return new WritingGradeResult
            {
                TaskResponseScore = 0,
                CoherenceCohesionScore = 0,
                LexicalResourceScore = 0,
                GrammarRangeScore = 0,
                TotalScore = 0,
                Strengths = "Không có bài viết để chấm điểm",
                Weaknesses = "Vui lòng nhập nội dung bài viết",
                Suggestions = "Hãy viết bài trước khi nộp"
            };
        }

        var systemPrompt = @"
Bạn là giám khảo chấm thi tiếng Anh VSTEP.
Chấm bài viết thang 10 theo 4 tiêu chí:
- Task Response (Nội dung) - 40%
- Coherence & Cohesion (Tổ chức) - 20%
- Lexical Resource (Từ vựng) - 20%
- Grammatical Range & Accuracy (Ngữ pháp) - 20%

Trả về JSON DUY NHẤT:
{
    ""taskResponseScore"": 7.0,
    ""coherenceCohesionScore"": 7.0,
    ""lexicalResourceScore"": 7.0,
    ""grammarRangeScore"": 7.0,
    ""strengths"": ""Điểm mạnh"",
    ""weaknesses"": ""Điểm yếu"",
    ""suggestions"": ""Gợi ý""
}";

        try
        {
            var response = await _openAIClient.GenerateContentAsync(essay, systemPrompt);
            string content = response.Choices.FirstOrDefault()?.Message?.Content ?? "{}";
            content = content.Replace("```json", "").Replace("```", "").Trim();

            var result = JsonSerializer.Deserialize<WritingGradeResult>(content);

            if (result != null)
            {
                result.TotalScore = Math.Round(
                    (result.TaskResponseScore * 0.4) +
                    (result.CoherenceCohesionScore * 0.2) +
                    (result.LexicalResourceScore * 0.2) +
                    (result.GrammarRangeScore * 0.2), 1);
                result.TotalScore = Math.Clamp(result.TotalScore, 0, 10);
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OpenAI Writing Error: {ex.Message}");
        }

        return GetFallbackWritingResult(essay);
    }

    // ============================================================
    // WRITING - BATCH
    // ============================================================

    public async Task<List<WritingGradeResult>> GradeWritingBatchAsync(
        List<(string essay, string prompt)> essays)
    {
        if (essays == null || essays.Count == 0)
            return new List<WritingGradeResult>();

        if (essays.Count == 1)
        {
            var result = await GradeWritingAsync(essays[0].essay, essays[0].prompt);
            return new List<WritingGradeResult> { result };
        }

        var systemPrompt = @"
Bạn là giám khảo chấm thi tiếng Anh VSTEP.
Chấm bài viết thang 10 theo 4 tiêu chí:
- Task Response (Nội dung) - 40%
- Coherence & Cohesion (Tổ chức) - 20%
- Lexical Resource (Từ vựng) - 20%
- Grammatical Range & Accuracy (Ngữ pháp) - 20%

Trả về JSON DUY NHẤT với cấu trúc:
{
    ""results"": [
        {
            ""taskResponseScore"": 7.0,
            ""coherenceCohesionScore"": 7.0,
            ""lexicalResourceScore"": 7.0,
            ""grammarRangeScore"": 7.0,
            ""strengths"": ""Điểm mạnh bài 1"",
            ""weaknesses"": ""Điểm yếu bài 1"",
            ""suggestions"": ""Gợi ý bài 1""
        }
    ]
}";

        var userPrompt = "Hãy chấm các bài viết sau:\n\n";
        for (int i = 0; i < essays.Count; i++)
        {
            userPrompt += $"===== BÀI {i + 1} =====\n";
            userPrompt += $"Đề bài: {essays[i].prompt}\n";
            userPrompt += $"Bài viết:\n{essays[i].essay}\n\n";
        }

        try
        {
            var response = await _openAIClient.GenerateContentAsync(userPrompt, systemPrompt);
            string content = response.Choices.FirstOrDefault()?.Message?.Content ?? "{}";
            content = content.Replace("```json", "").Replace("```", "").Trim();

            var batchResult = JsonSerializer.Deserialize<BatchWritingResponse>(content);

            if (batchResult?.Results != null && batchResult.Results.Count == essays.Count)
            {
                foreach (var result in batchResult.Results)
                {
                    result.TotalScore = Math.Round(
                        (result.TaskResponseScore * 0.4) +
                        (result.CoherenceCohesionScore * 0.2) +
                        (result.LexicalResourceScore * 0.2) +
                        (result.GrammarRangeScore * 0.2), 1);
                    result.TotalScore = Math.Clamp(result.TotalScore, 0, 10);
                }
                return batchResult.Results;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Batch Writing Error: {ex.Message}");
        }

        var fallbackResults = new List<WritingGradeResult>();
        foreach (var essay in essays)
        {
            fallbackResults.Add(await GradeWritingAsync(essay.essay, essay.prompt));
        }
        return fallbackResults;
    }

    // ============================================================
    // SPEAKING
    // ============================================================

    public async Task<SpeakingGradeResult> GradeSpeakingAsync(byte[] audioData, string question)
    {
        var transcript = await SpeechToTextAsync(audioData);
        return await GradeSpeakingContentAsync(transcript, question);
    }

    public async Task<SpeakingGradeResult> GradeSpeakingContentAsync(string transcript, string question)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            Console.WriteLine("❌ Transcript is empty or null");
            throw new ArgumentException("Transcript cannot be empty", nameof(transcript));
        }

        var systemPrompt = @"
Bạn là giám khảo chấm thi nói tiếng Anh VSTEP.
Chấm bài nói thang điểm 10 theo 4 tiêu chí:
- Content (Nội dung) - 40%
- Organization (Tổ chức) - 30%
- Grammar (Ngữ pháp) - 15%
- Vocabulary (Từ vựng) - 15%

Trả về JSON DUY NHẤT:
{
    ""contentScore"": 7.0,
    ""organizationScore"": 7.0,
    ""grammarScore"": 7.0,
    ""vocabularyScore"": 7.0,
    ""totalScore"": 7.0,
    ""strengths"": ""Điểm mạnh"",
    ""weaknesses"": ""Điểm yếu"",
    ""suggestions"": ""Gợi ý""
}";

        var userPrompt = $"Câu hỏi: {question}\n\nCâu trả lời:\n{transcript}";

        try
        {
            var response = await _openAIClient.GenerateContentAsync(userPrompt, systemPrompt);
            string content = response.Choices.FirstOrDefault()?.Message?.Content ?? "{}";
            content = content.Replace("```json", "").Replace("```", "").Trim();

            var result = JsonSerializer.Deserialize<SpeakingGradeResult>(content);

            if (result != null)
            {
                result.TotalScore = Math.Round(
                    (result.ContentScore * 0.4) +
                    (result.OrganizationScore * 0.3) +
                    (result.GrammarScore * 0.15) +
                    (result.VocabularyScore * 0.15), 1);
                result.TotalScore = Math.Clamp(result.TotalScore, 0, 10);
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Speaking content error: {ex.Message}");
        }

        // ✅ Ném exception khi không có kết quả từ AI
        throw new Exception("Failed to grade speaking content");
    }

    public async Task<string> SpeechToTextAsync(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
        {
            Console.WriteLine("❌ Audio data is null or empty");
            throw new ArgumentException("Audio data is null or empty", nameof(audioData));
        }

        Console.WriteLine($"🎤 SpeechToTextAsync: Processing {audioData.Length} bytes");
        var transcript = await _whisperClient.TranscribeAsync(audioData);

        if (string.IsNullOrWhiteSpace(transcript))
        {
            Console.WriteLine("❌ Whisper returned empty transcript");
            throw new Exception("Whisper returned empty transcript");
        }

        Console.WriteLine($"✅ SpeechToTextAsync: Got transcript ({transcript.Length} chars)");
        return transcript;
    }

    // ============================================================
    // UTILITY METHODS (CHỈ DÙNG CHO WRITING FALLBACK)
    // ============================================================

    private static WritingGradeResult GetFallbackWritingResult(string essay)
    {
        int wordCount = essay?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        double baseScore = wordCount >= 250 ? 7.5 : wordCount >= 200 ? 6.5 : wordCount >= 100 ? 5.0 : 3.5;

        return new WritingGradeResult
        {
            TaskResponseScore = baseScore,
            CoherenceCohesionScore = baseScore - 0.5,
            LexicalResourceScore = baseScore,
            GrammarRangeScore = baseScore - 0.5,
            TotalScore = baseScore,
            Strengths = "Bài viết cơ bản",
            Weaknesses = "Cần cải thiện độ dài và cấu trúc",
            Suggestions = "Viết dài hơn và phát triển ý chi tiết"
        };
    }
}