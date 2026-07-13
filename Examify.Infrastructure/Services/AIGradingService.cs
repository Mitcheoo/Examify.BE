// 📁 Examify.Infrastructure/Services/AIGradingService.cs
using Examify.Core.Interfaces;
using Examify.Infrastructure.External;
using System.Text;
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
    // HELPER: ĐẾM TỪ
    // ============================================================

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
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
                Suggestions = "Hãy viết bài trước khi nộp",
                DetailedFeedback = new List<WritingDetailedFeedback>()
            };
        }

        // ============================================================
        // KIỂM TRA SỐ TỪ - ÁP DỤNG ĐIỂM THẤP
        // ============================================================
        int wordCount = CountWords(essay);

        // Nếu < 30 từ → điểm 1 TẤT CẢ các tiêu chí
        if (wordCount < 30)
        {
            return new WritingGradeResult
            {
                TaskResponseScore = 1.0,
                CoherenceCohesionScore = 1.0,
                LexicalResourceScore = 1.0,
                GrammarRangeScore = 1.0,
                TotalScore = 1.0,
                Strengths = "Bài viết quá ngắn, không đủ để đánh giá",
                Weaknesses = $"Bài viết chỉ có {wordCount} từ, cần ít nhất 50 từ",
                Suggestions = "Hãy phát triển ý tưởng chi tiết hơn, viết dài hơn",
                DetailedFeedback = new List<WritingDetailedFeedback>()
            };
        }

        // Nếu < 50 từ → điểm tối đa 3
        bool isTooShort = wordCount < 50;

        var systemPrompt = @"
Bạn là giám khảo chuyên nghiệp chấm thi VIẾT tiếng Anh VSTEP (trình độ B1-C1).
Nhiệm vụ: Chấm bài viết của thí sinh như một giám khảo thật, KHÁCH QUAN và CHI TIẾT.

===============================
📊 THANG ĐIỂM 10 (làm tròn 1 chữ số thập phân)
===============================
9.0-10.0: Xuất sắc - Đáp ứng hoàn hảo mọi tiêu chí
8.0-8.9:  Rất tốt - Đáp ứng tốt hầu hết tiêu chí
7.0-7.9:  Tốt - Đáp ứng tốt các tiêu chí chính
6.0-6.9:  Khá - Đáp ứng cơ bản, còn thiếu sót
5.0-5.9:  Trung bình - Đáp ứng một phần yêu cầu
4.0-4.9:  Yếu - Còn nhiều lỗi và thiếu ý
3.0-3.9:  Rất yếu - Cực kỳ nhiều lỗi, khó hiểu
2.0-2.9:  Kém - Không đáp ứng yêu cầu cơ bản
1.0-1.9:  Rất kém - Trả lời được rất ít, dưới 30 từ
0.0-0.9:  Hoàn toàn không đạt - Không viết được gì

===============================
📚 4 TIÊU CHÍ CHẤM ĐIỂM
===============================

1. Task Response (40%):
   ✓ Thí sinh có trả lời đúng trọng tâm câu hỏi không?
   ✓ Ý tưởng có phát triển đầy đủ, logic không?
   ✓ Có đưa ra ví dụ/dẫn chứng cụ thể không?
   ✓ Số từ có đạt yêu cầu không?

2. Coherence & Cohesion (20%):
   ✓ Bố cục bài viết có rõ ràng (mở bài, thân bài, kết bài) không?
   ✓ Các đoạn văn có kết nối mạch lạc không?
   ✓ Sử dụng từ nối (However, Moreover, Therefore...) có hợp lý không?

3. Lexical Resource (20%):
   ✓ Từ vựng có đa dạng và chính xác không?
   ✓ Có sử dụng được collocations, idioms, phrasal verbs không?
   ✓ Có lặp từ quá nhiều không?

4. Grammatical Range & Accuracy (20%):
   ✓ Cấu trúc câu có đa dạng không? (đơn, ghép, phức)
   ✓ Ngữ pháp có chính xác không? (thì, mạo từ, giới từ, câu điều kiện...)
   ✓ Có mắc lỗi chính tả hoặc dấu câu không?

===============================
📤 YÊU CẦU TRẢ VỀ JSON DUY NHẤT
===============================
Trả về JSON DUY NHẤT (không có bất kỳ text nào khác ngoài JSON):

{
    ""taskResponseScore"": 7.5,
    ""coherenceCohesionScore"": 7.0,
    ""lexicalResourceScore"": 7.0,
    ""grammarRangeScore"": 7.5,
    ""totalScore"": 7.3,
    ""strengths"": ""📌 Điểm mạnh:\n- [dẫn chứng cụ thể từ bài viết]\n- [dẫn chứng cụ thể từ bài viết]"",
    ""weaknesses"": ""⚠️ Điểm yếu:\n- [trích dẫn nguyên văn câu sai, mô tả vấn đề]\n- [trích dẫn nguyên văn câu sai, mô tả vấn đề]"",
    ""suggestions"": ""💡 Gợi ý cải thiện:\n1. [hướng dẫn cụ thể, chi tiết]\n2. [hướng dẫn cụ thể, chi tiết]"",
    ""detailedFeedback"": [
        {
            ""issue"": ""[Mô tả ngắn gọn vấn đề]"",
            ""sentence"": ""[Trích dẫn nguyên văn câu có lỗi]"",
            ""suggestion"": ""[Đề xuất cách sửa cụ thể]""
        }
    ]
}

⚠️ LƯU Ý QUAN TRỌNG:
- Điểm số PHẢI KHÁCH QUAN, dựa trên BẰNG CHỨNG trong bài viết
- Nhận xét phải CỤ THỂ, trích dẫn nguyên văn câu trong bài viết
- Gợi ý cải thiện phải THỰC TẾ, CÓ THỂ ÁP DỤNG NGAY
- detailedFeedback TỐI THIỂU 3 lỗi, TỐI ĐA 10 lỗi
- Sử dụng emoji giúp feedback sinh động hơn

🔴 QUY TẮC PHÂN BIỆT ĐIỂM MẠNH - ĐIỂM YẾU:
- Strengths: CHỈ ĐƯA RA ĐIỂM TÍCH CỰC. Ví dụ: 'Bài viết có bố cục rõ ràng', 'Từ vựng đa dạng', 'Ý tưởng phát triển tốt'
- Weaknesses: ĐƯA RA ĐIỂM CẦN CẢI THIỆN. Ví dụ: 'Bài viết quá ngắn', 'Sai ngữ pháp', 'Lặp từ', 'Thiếu dẫn chứng'
- KHÔNG ĐƯỢC ĐƯA NỘI DUNG TIÊU CỰC VÀO Strengths. 'Bài viết quá ngắn' là ĐIỂM YẾU, không phải điểm mạnh.

🔴 QUY TẮC CHO BÀI VIẾT NGẮN:
- Nếu bài viết < 30 từ: Điểm 1.0 tất cả tiêu chí, Strengths = 'Bài viết quá ngắn để đánh giá điểm mạnh', Weaknesses = 'Bài viết chỉ có X từ, cần ít nhất 50 từ', detailedFeedback = []
- Nếu bài viết 30-50 từ: Điểm tối đa 3.0, Strengths = 'Bài viết đã có ý tưởng cơ bản nhưng cần phát triển thêm'
- Nếu bài viết >= 50 từ: Chấm bình thường, detailedFeedback tối thiểu 3 lỗi
";

        try
        {
            var userPrompt = $"Đề bài: {prompt}\n\nBài viết của thí sinh:\n{essay}";

            var response = await _openAIClient.GenerateContentAsync(userPrompt, systemPrompt);

            if (response == null || string.IsNullOrEmpty(response.Content))
            {
                Console.WriteLine("❌ OpenAI returned empty response");
                return GetFallbackWritingResult(essay);
            }

            var content = response.Content.Replace("```json", "").Replace("```", "").Trim();
            Console.WriteLine($"📥 OpenAI Raw Response: {content.Substring(0, Math.Min(200, content.Length))}...");

            var result = JsonSerializer.Deserialize<WritingGradeResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null)
            {
                result.DetailedFeedback ??= new List<WritingDetailedFeedback>();

                // Giới hạn số lượng detailedFeedback (3-10 lỗi)
                if (result.DetailedFeedback.Count > 10)
                {
                    result.DetailedFeedback = result.DetailedFeedback.Take(10).ToList();
                }

                // Đảm bảo các giá trị trong khoảng 0-10
                result.TaskResponseScore = Math.Clamp(result.TaskResponseScore, 0, 10);
                result.CoherenceCohesionScore = Math.Clamp(result.CoherenceCohesionScore, 0, 10);
                result.LexicalResourceScore = Math.Clamp(result.LexicalResourceScore, 0, 10);
                result.GrammarRangeScore = Math.Clamp(result.GrammarRangeScore, 0, 10);

                // Nếu bài viết quá ngắn (< 50 từ), giới hạn điểm tối đa 3
                if (isTooShort)
                {
                    result.TaskResponseScore = Math.Min(result.TaskResponseScore, 3.0);
                    result.CoherenceCohesionScore = Math.Min(result.CoherenceCohesionScore, 3.0);
                    result.LexicalResourceScore = Math.Min(result.LexicalResourceScore, 3.0);
                    result.GrammarRangeScore = Math.Min(result.GrammarRangeScore, 3.0);
                }

                result.TotalScore = Math.Round(
                    (result.TaskResponseScore * 0.4) +
                    (result.CoherenceCohesionScore * 0.2) +
                    (result.LexicalResourceScore * 0.2) +
                    (result.GrammarRangeScore * 0.2), 1);
                result.TotalScore = Math.Clamp(result.TotalScore, 0, 10);

                Console.WriteLine($"✅ AI Result: TaskResponse={result.TaskResponseScore}, Total={result.TotalScore}");
                return result;
            }
            else
            {
                Console.WriteLine("❌ Failed to parse OpenAI response");
                return GetFallbackWritingResult(essay);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OpenAI Writing Error: {ex.Message}");
            return GetFallbackWritingResult(essay);
        }
    }

    // ============================================================
    // WRITING - BATCH
    // ============================================================

    public async Task<List<WritingGradeResult>> GradeWritingBatchAsync(
        List<(string essay, string prompt)> essays)
    {
        if (essays == null || essays.Count == 0)
            return new List<WritingGradeResult>();

        Console.WriteLine($"📊 Batch grading {essays.Count} essays");

        var results = new List<WritingGradeResult>();

        foreach (var (essay, prompt) in essays)
        {
            var result = await GradeWritingAsync(essay, prompt);
            results.Add(result);
        }

        return results;
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

        // ============================================================
        // KIỂM TRA SỐ TỪ CỦA TRANSCRIPT
        // ============================================================
        int wordCount = CountWords(transcript);

        // Nếu < 10 từ → điểm 1 TẤT CẢ các tiêu chí
        if (wordCount < 10)
        {
            return new SpeakingGradeResult
            {
                ContentScore = 1.0,
                OrganizationScore = 1.0,
                GrammarScore = 1.0,
                VocabularyScore = 1.0,
                TotalScore = 1.0,
                Strengths = "Không đủ nội dung để đánh giá",
                Weaknesses = $"Câu trả lời chỉ có {wordCount} từ, quá ngắn",
                Suggestions = "Hãy nói dài hơn, phát triển ý tưởng chi tiết",
                ErrorAnalysis = new List<SpeakingErrorAnalysis>()
            };
        }

        // Nếu < 20 từ → điểm tối đa 3
        bool isTooShort = wordCount < 20;

        var systemPrompt = @"
Bạn là giám khảo chuyên nghiệp chấm thi NÓI tiếng Anh VSTEP (trình độ B1-C1).
Nhiệm vụ: Dựa trên TRANSCRIPT (văn bản từ giọng nói) để chấm bài nói của thí sinh.

===============================
📊 THANG ĐIỂM 10 (làm tròn 1 chữ số thập phân)
===============================
9.0-10.0: Xuất sắc - Trôi chảy, tự nhiên, ít sai sót
8.0-8.9:  Rất tốt - Nói rõ ràng, ít sai sót
7.0-7.9:  Tốt - Diễn đạt tốt, còn một vài lỗi nhỏ
6.0-6.9:  Khá - Diễn đạt được ý nhưng còn lỗi
5.0-5.9:  Trung bình - Nhiều lỗi, còn ngập ngừng
4.0-4.9:  Yếu - Khó hiểu, nhiều lỗi ngữ pháp
3.0-3.9:  Rất yếu - Cực kỳ nhiều lỗi
2.0-2.9:  Kém - Không đáp ứng yêu cầu cơ bản
1.0-1.9:  Rất kém - Trả lời được rất ít, dưới 10 từ
0.0-0.9:  Hoàn toàn không đạt - Không nói được gì

===============================
🎤 4 TIÊU CHÍ CHẤM ĐIỂM
===============================

1. Content (40%):
   ✓ Có trả lời đúng câu hỏi không?
   ✓ Ý tưởng có phát triển đầy đủ, thuyết phục không?
   ✓ Có đưa ra quan điểm cá nhân rõ ràng không?

2. Organization (30%):
   ✓ Bài nói có mở đầu, phát triển, kết luận rõ ràng không?
   ✓ Các ý có được sắp xếp logic không?
   ✓ Có sử dụng từ nối để liên kết ý không?

3. Grammar (15%):
   ✓ Ngữ pháp có chính xác không?
   ✓ Có sử dụng được cấu trúc câu đa dạng không?
   ✓ Có mắc lỗi ngữ pháp cơ bản không?

4. Vocabulary (15%):
   ✓ Từ vựng có đa dạng và phù hợp với chủ đề không?
   ✓ Có sử dụng được từ vựng nâng cao không?
   ✓ Có lặp từ quá nhiều không?

===============================
📤 YÊU CẦU TRẢ VỀ JSON DUY NHẤT
===============================
Trả về JSON DUY NHẤT (không có bất kỳ text nào khác ngoài JSON):

{
    ""contentScore"": 7.0,
    ""organizationScore"": 7.0,
    ""grammarScore"": 7.0,
    ""vocabularyScore"": 7.0,
    ""totalScore"": 7.0,
    ""strengths"": ""📌 Điểm mạnh:\n- [trích dẫn câu hay từ transcript]\n- [trích dẫn câu hay từ transcript]"",
    ""weaknesses"": ""⚠️ Điểm yếu:\n- [trích dẫn nguyên văn câu sai]\n- [trích dẫn nguyên văn câu sai]"",
    ""suggestions"": ""💡 Gợi ý cải thiện:\n1. [hướng dẫn cụ thể, chi tiết]\n2. [hướng dẫn cụ thể, chi tiết]"",
    ""errorAnalysis"": [
        {
            ""transcript"": ""[Trích dẫn nguyên văn câu sai]"",
            ""issue"": ""[Mô tả ngắn gọn vấn đề]"",
            ""correction"": ""[Đề xuất cách sửa cụ thể]""
        }
    ]
}

⚠️ LƯU Ý QUAN TRỌNG:
- Phân tích transcript THỰC TẾ, KHÁCH QUAN
- Chỉ phân tích được lỗi NGỮ PHÁP và TỪ VỰNG (không phân tích phát âm)
- Trích dẫn nguyên văn câu sai và đưa ra cách sửa
- errorAnalysis TỐI THIỂU 2 lỗi, TỐI ĐA 8 lỗi
- Gợi ý PHẢI THỰC TẾ và CÓ THỂ ÁP DỤNG NGAY
";

        try
        {
            var userPrompt = $"Câu hỏi: {question}\n\nCâu trả lời của thí sinh:\n{transcript}";

            var response = await _openAIClient.GenerateContentAsync(userPrompt, systemPrompt);

            if (response == null || string.IsNullOrEmpty(response.Content))
            {
                throw new Exception("OpenAI returned empty response");
            }

            var content = response.Content.Replace("```json", "").Replace("```", "").Trim();
            var result = JsonSerializer.Deserialize<SpeakingGradeResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null)
            {
                result.ErrorAnalysis ??= new List<SpeakingErrorAnalysis>();

                // Giới hạn số lượng errorAnalysis (2-8 lỗi)
                if (result.ErrorAnalysis.Count > 8)
                {
                    result.ErrorAnalysis = result.ErrorAnalysis.Take(8).ToList();
                }

                // Đảm bảo các giá trị trong khoảng 0-10
                result.ContentScore = Math.Clamp(result.ContentScore, 0, 10);
                result.OrganizationScore = Math.Clamp(result.OrganizationScore, 0, 10);
                result.GrammarScore = Math.Clamp(result.GrammarScore, 0, 10);
                result.VocabularyScore = Math.Clamp(result.VocabularyScore, 0, 10);

                // Nếu transcript quá ngắn (< 20 từ), giới hạn điểm tối đa 3
                if (isTooShort)
                {
                    result.ContentScore = Math.Min(result.ContentScore, 3.0);
                    result.OrganizationScore = Math.Min(result.OrganizationScore, 3.0);
                    result.GrammarScore = Math.Min(result.GrammarScore, 3.0);
                    result.VocabularyScore = Math.Min(result.VocabularyScore, 3.0);
                }

                result.TotalScore = Math.Round(
                    (result.ContentScore * 0.4) +
                    (result.OrganizationScore * 0.3) +
                    (result.GrammarScore * 0.15) +
                    (result.VocabularyScore * 0.15), 1);
                result.TotalScore = Math.Clamp(result.TotalScore, 0, 10);

                Console.WriteLine($"✅ Speaking AI Result: Content={result.ContentScore}, Total={result.TotalScore}");
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Speaking content error: {ex.Message}");
        }

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
    // UTILITY METHODS
    // ============================================================

    private static WritingGradeResult GetFallbackWritingResult(string essay)
    {
        int wordCount = CountWords(essay);
        double baseScore = wordCount >= 250 ? 7.5 : wordCount >= 200 ? 6.5 : wordCount >= 100 ? 5.0 : 3.5;

        return new WritingGradeResult
        {
            TaskResponseScore = baseScore,
            CoherenceCohesionScore = baseScore - 0.5,
            LexicalResourceScore = baseScore,
            GrammarRangeScore = baseScore - 0.5,
            TotalScore = baseScore,
            Strengths = "Bài viết có cấu trúc cơ bản",
            Weaknesses = "Cần cải thiện độ dài và phát triển ý tưởng",
            Suggestions = "Viết dài hơn, sử dụng từ nối đa dạng và phát triển luận điểm chi tiết",
            DetailedFeedback = new List<WritingDetailedFeedback>()
        };
    }
}