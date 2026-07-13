//Examify.Application/Cqrs/Commands/Speaking/SubmitSpeakingCommandHandler.cs
using Examify.Application.DTOs.Submissions;
using Examify.Core.Entities;
using Examify.Core.Enums;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Examify.Application.Cqrs.Commands.Speaking;

public sealed class SubmitSpeakingCommandHandler : IRequestHandler<SubmitSpeakingCommand, SubmissionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIGradingService _aiGradingService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubmitSpeakingCommandHandler(
        IUnitOfWork unitOfWork,
        IAIGradingService aiGradingService,
        IFileStorageService fileStorageService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _aiGradingService = aiGradingService;
        _fileStorageService = fileStorageService;
        _httpContextAccessor = httpContextAccessor;
    }

    // ============================================================
    // HELPER: ĐẾM TỪ
    // ============================================================

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public async Task<SubmissionDetailDto> Handle(SubmitSpeakingCommand request, CancellationToken cancellationToken)
    {
        // ============================================================
        // 1. AUTHENTICATION - LẤY USER ID TỪ TOKEN
        // ============================================================
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new Exception("User not authenticated");

        var userId = Guid.Parse(userIdClaim);
        Console.WriteLine($"👤 User ID from token: {userId}");

        // ============================================================
        // 2. VALIDATE - KIỂM TRA DỮ LIỆU ĐẦU VÀO
        // ============================================================

        // 2.1. Kiểm tra có audio files không
        if (request.AudioFiles == null || request.AudioFiles.Count == 0)
        {
            throw new BadRequestException("Bạn phải ghi âm câu trả lời trước khi nộp bài.");
        }

        // 2.2. Lấy Exercise
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
        {
            throw new NotFoundException($"Không tìm thấy bài thi với ID: {request.ExerciseId}");
        }

        // 2.3. Lấy danh sách câu hỏi Speaking
        var questions = await _unitOfWork.SpeakingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted);

        var questionList = questions.OrderBy(q => q.OrderNumber).ToList();
        if (questionList.Count == 0)
        {
            throw new NotFoundException("Không tìm thấy câu hỏi Speaking cho bài thi này.");
        }

        // 2.4. Kiểm tra số lượng audio khớp với số câu hỏi
        if (request.AudioFiles.Count != questionList.Count)
        {
            throw new BadRequestException(
                $"Số lượng file audio ({request.AudioFiles.Count}) không khớp với số câu hỏi ({questionList.Count}). " +
                $"Vui lòng ghi âm đủ {questionList.Count} câu hỏi.");
        }

        Console.WriteLine($"📊 Found {questionList.Count} speaking questions");
        Console.WriteLine($"🎤 Processing {request.AudioFiles.Count} audio files");

        // ============================================================
        // 3. XỬ LÝ TỪNG AUDIO FILE
        // ============================================================
        var audioUrls = new List<string>();
        var transcripts = new Dictionary<Guid, string>();
        var wordCounts = new Dictionary<Guid, int>();
        var details = new List<SubmissionAnswerDetailDto>();
        var allAiResults = new List<SpeakingGradeResult>();
        double totalScore = 0;

        for (int i = 0; i < request.AudioFiles.Count; i++)
        {
            var audioFile = request.AudioFiles[i];
            var question = questionList[i];

            Console.WriteLine($"--- Processing Question {i + 1}/{questionList.Count} ---");
            Console.WriteLine($"📁 File size: {audioFile.Length} bytes");
            Console.WriteLine($"📝 Question: {question.QuestionText}");

            // ============================================================
            // 3.1. ĐỌC FILE THÀNH BYTE ARRAY
            // ============================================================
            byte[] audioData;
            using (var ms = new MemoryStream())
            {
                await audioFile.CopyToAsync(ms, cancellationToken);
                audioData = ms.ToArray();
            }

            if (audioData.Length == 0)
            {
                throw new BadRequestException($"File audio cho câu hỏi {i + 1} bị rỗng (0 bytes).");
            }

            Console.WriteLine($"🎵 Audio data size: {audioData.Length} bytes");

            // ============================================================
            // 3.2. UPLOAD FILE (DÙNG BYTE ARRAY)
            // ============================================================
            string audioUrl;
            try
            {
                var fileName = $"speaking_{userId}_{i + 1}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.webm";
                audioUrl = await _fileStorageService.UploadFileAsync(audioData, "speaking-audios", fileName);
                audioUrls.Add(audioUrl);
                Console.WriteLine($"✅ Uploaded audio: {audioUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to upload audio: {ex.Message}");
                throw new Exception($"Không thể upload file audio cho câu hỏi {i + 1}: {ex.Message}");
            }

            // ============================================================
            // 3.3. GỌI WHISPER API (DÙNG BYTE ARRAY)
            // ============================================================
            string transcript;
            try
            {
                transcript = await _aiGradingService.SpeechToTextAsync(audioData);
                Console.WriteLine($"🎤 Whisper transcript: '{transcript}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Whisper failed: {ex.Message}");
                transcript = "Không nhận diện được giọng nói. Vui lòng kiểm tra microphone.";
                Console.WriteLine($"⚠️ Using fallback transcript for question {i + 1}");
            }

            // ============================================================
            // 3.4. LƯU TRANSCRIPT VÀ ĐẾM SỐ TỪ
            // ============================================================
            transcripts[question.Id] = transcript ?? string.Empty;
            wordCounts[question.Id] = CountWords(transcript);
            Console.WriteLine($"📝 Transcript saved: '{transcript}' (length: {transcript?.Length ?? 0}, words: {wordCounts[question.Id]})");

            // ============================================================
            // 3.5. GỌI AI CHẤM ĐIỂM - CÓ FALLBACK
            // ============================================================
            SpeakingGradeResult aiResult;
            try
            {
                // Nếu transcript rỗng hoặc quá ngắn, dùng text mặc định để AI vẫn chấm
                var textToGrade = transcript;
                if (string.IsNullOrWhiteSpace(transcript) || transcript.Replace(".", "").Replace(" ", "").Trim().Length < 2)
                {
                    textToGrade = "The candidate's response could not be clearly understood. Please evaluate based on the limited content available.";
                    Console.WriteLine($"⚠️ Using fallback text for AI grading: '{textToGrade}'");
                }

                aiResult = await _aiGradingService.GradeSpeakingContentAsync(textToGrade, question.QuestionText);

                if (aiResult == null)
                {
                    throw new Exception("AI returned null result");
                }

                Console.WriteLine($"🤖 AI Score: {aiResult.TotalScore}/10");
                Console.WriteLine($"   Content: {aiResult.ContentScore}/10");
                Console.WriteLine($"   Organization: {aiResult.OrganizationScore}/10");
                Console.WriteLine($"   Grammar: {aiResult.GrammarScore}/10");
                Console.WriteLine($"   Vocabulary: {aiResult.VocabularyScore}/10");
                Console.WriteLine($"   Error Analysis: {aiResult.ErrorAnalysis?.Count ?? 0} errors");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AI grading failed: {ex.Message}");
                aiResult = new SpeakingGradeResult
                {
                    ContentScore = 2.0,
                    OrganizationScore = 2.0,
                    GrammarScore = 2.0,
                    VocabularyScore = 2.0,
                    TotalScore = 2.0,
                    Strengths = "Không có đủ dữ liệu để đánh giá.",
                    Weaknesses = "Không nhận diện được giọng nói hoặc nội dung quá ngắn.",
                    Suggestions = "Vui lòng ghi âm trong môi trường yên tĩnh, nói to và rõ ràng hơn.",
                    ErrorAnalysis = new List<SpeakingErrorAnalysis>()
                };
                Console.WriteLine($"⚠️ Using fallback AI score: 2.0/10 for question {i + 1}");
            }

            allAiResults.Add(aiResult);
            totalScore += aiResult.TotalScore;

            // ============================================================
            // 3.6. LƯU KẾT QUẢ CHI TIẾT
            // ============================================================
            details.Add(new SubmissionAnswerDetailDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                OrderNumber = question.OrderNumber,
                UserAnswer = transcript ?? "Không nhận diện được giọng nói",
                CorrectAnswer = null,
                IsCorrect = aiResult.TotalScore >= 5.0,
                AiScore = aiResult.TotalScore,
                AiFeedback = FormatAiFeedback(aiResult, wordCounts[question.Id]),
                Explanation = aiResult.Suggestions
            });

            Console.WriteLine($"✅ Question {i + 1} completed with score: {aiResult.TotalScore}/10");
        }

        // ============================================================
        // 4. TÍNH ĐIỂM TRUNG BÌNH
        // ============================================================
        var averageScore = Math.Round(totalScore / questionList.Count, 1);
        averageScore = Math.Clamp(averageScore, 0, 10);
        var finalScore = (short)Math.Round(averageScore);

        Console.WriteLine($"📊 Average score: {averageScore}/10");
        Console.WriteLine($"📊 Final score: {finalScore}/10");

        // ============================================================
        // 5. TẠO SUBMISSION
        // ============================================================
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExerciseId = request.ExerciseId,
            SkillType = (int)SkillType.Speaking,
            TotalScore = finalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = 0,
            TimeSpentSeconds = request.TimeSpentSeconds,
            AudioUrl = string.Join(",", audioUrls),
            Transcript = JsonSerializer.Serialize(transcripts),
            AnswerJson = JsonSerializer.Serialize(transcripts),
            ResultJson = JsonSerializer.Serialize(details),
            AiFeedback = JsonSerializer.Serialize(new
            {
                Summary = "Speaking graded by AI",
                AudioUrls = audioUrls,
                Transcripts = transcripts,
                WordCounts = wordCounts, // ✅ THÊM: Lưu số từ
                Details = details,
                Results = allAiResults.Select(r => new
                {
                    r.ContentScore,
                    r.OrganizationScore,
                    r.GrammarScore,
                    r.VocabularyScore,
                    r.TotalScore,
                    r.Strengths,
                    r.Weaknesses,
                    r.Suggestions,
                    ErrorAnalysis = r.ErrorAnalysis // ✅ THÊM: Lưu error analysis
                })
            }),
            IsGraded = true,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        Console.WriteLine($"✅ Submission created: {submission.Id}");

        // ============================================================
        // 6. CẬP NHẬT SESSION (NẾU CÓ)
        // ============================================================
        if (request.SessionId.HasValue && request.SessionId.Value != Guid.Empty)
        {
            var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId.Value);
            if (session != null)
            {
                session.SpeakingSubmissionId = submission.Id;
                session.SpeakingTimeSpent = request.TimeSpentSeconds;
                session.SpeakingExerciseId = request.ExerciseId;
                session.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.FullTestSessions.UpdateAsync(session);
                Console.WriteLine($"✅ Updated session {session.Id} with SpeakingSubmissionId: {submission.Id}");
            }
            else
            {
                Console.WriteLine($"⚠️ Session not found: {request.SessionId.Value}");
            }
        }

        // ============================================================
        // 7. CẬP NHẬT ATTEMPT COUNT
        // ============================================================
        exercise.AttemptCount++;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        // ============================================================
        // 8. LƯU THAY ĐỔI
        // ============================================================
        await _unitOfWork.SaveChangesAsync();
        Console.WriteLine("✅ All changes saved to database");

        // ============================================================
        // 9. TRẢ VỀ KẾT QUẢ
        // ============================================================
        return new SubmissionDetailDto
        {
            Id = submission.Id,
            UserId = submission.UserId,
            ExerciseId = submission.ExerciseId,
            ExerciseTitle = exercise.Title,
            Skill = (int)SkillType.Speaking,
            SkillName = SkillType.Speaking.ToString(),
            TotalScore = averageScore,
            TotalQuestions = questionList.Count,
            CorrectCount = 0,
            SubmittedAt = submission.SubmittedAt,
            AudioUrl = submission.AudioUrl,
            Transcript = submission.Transcript,
            AnswerJson = submission.AnswerJson,
            Details = details,
            AiFeedback = new
            {
                Summary = "Speaking graded by AI",
                AudioUrls = audioUrls,
                Transcripts = transcripts,
                WordCounts = wordCounts,
                Details = details,
                Results = allAiResults.Select(r => new
                {
                    r.ContentScore,
                    r.OrganizationScore,
                    r.GrammarScore,
                    r.VocabularyScore,
                    r.TotalScore,
                    r.Strengths,
                    r.Weaknesses,
                    r.Suggestions,
                    ErrorAnalysis = r.ErrorAnalysis
                })
            }
        };
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    private static string FormatAiFeedback(SpeakingGradeResult result, int wordCount)
    {
        if (result == null) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"🎯 Điểm tổng: {result.TotalScore}/10");
        sb.AppendLine($"📝 Số từ: {wordCount}");
        sb.AppendLine();
        sb.AppendLine("📊 Chi tiết các tiêu chí:");
        sb.AppendLine($"   • Nội dung: {result.ContentScore}/10");
        sb.AppendLine($"   • Tổ chức: {result.OrganizationScore}/10");
        sb.AppendLine($"   • Ngữ pháp: {result.GrammarScore}/10");
        sb.AppendLine($"   • Từ vựng: {result.VocabularyScore}/10");
        sb.AppendLine();
        sb.AppendLine($"✅ Điểm mạnh:\n{result.Strengths}");
        sb.AppendLine();
        sb.AppendLine($"⚠️ Điểm yếu:\n{result.Weaknesses}");
        sb.AppendLine();
        sb.AppendLine($"💡 Gợi ý cải thiện:\n{result.Suggestions}");

        // ✅ THÊM: Error Analysis
        if (result.ErrorAnalysis != null && result.ErrorAnalysis.Any())
        {
            sb.AppendLine();
            sb.AppendLine("📝 Phân tích lỗi chi tiết:");
            foreach (var error in result.ErrorAnalysis)
            {
                sb.AppendLine($"   • Câu nói: \"{error.Transcript}\"");
                sb.AppendLine($"     Vấn đề: {error.Issue}");
                sb.AppendLine($"     Sửa: \"{error.Correction}\"");
            }
        }

        return sb.ToString();
    }
}