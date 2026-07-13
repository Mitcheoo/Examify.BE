// 📁 Examify.Application/Cqrs/Commands/Writing/SubmitWritingCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Submissions;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Examify.Application.Cqrs.Commands.Writing;

public class SubmitWritingCommandHandler : IRequestHandler<SubmitWritingCommand, SubmissionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIGradingService _aiGradingService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubmitWritingCommandHandler(
        IUnitOfWork unitOfWork,
        IAIGradingService aiGradingService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _aiGradingService = aiGradingService;
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

    public async Task<SubmissionDetailDto> Handle(SubmitWritingCommand request, CancellationToken cancellationToken)
    {
        //  LẤY USER ID TỪ TOKEN
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new Exception("User not authenticated");
        var userId = Guid.Parse(userIdClaim);
        Console.WriteLine($"👤 User ID from token: {userId}");

        // 1. Lấy tất cả câu hỏi Writing
        var questions = await _unitOfWork.WritingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted);

        var questionList = questions.OrderBy(q => q.OrderNumber).ToList();

        if (questionList.Count == 0)
            throw new NotFoundException("Writing questions not found");

        // 2. Lấy thông tin Exercise
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        // 3. Chuẩn bị bài viết cho AI
        var essayTexts = new Dictionary<Guid, string>();
        var essaysForAI = new List<(string essay, string prompt)>();
        var wordCounts = new Dictionary<Guid, int>();

        foreach (var question in questionList)
        {
            string essayText = GetEssayText(request, question);
            essayTexts[question.Id] = essayText;
            wordCounts[question.Id] = CountWords(essayText);
            essaysForAI.Add((essayText, question.PromptText));
        }

        // 4. Gọi AI chấm BATCH
        var aiResults = await _aiGradingService.GradeWritingBatchAsync(essaysForAI);

        if (aiResults == null || aiResults.Count == 0)
        {
            aiResults = questionList.Select(q => new WritingGradeResult
            {
                TaskResponseScore = 5.0,
                CoherenceCohesionScore = 5.0,
                LexicalResourceScore = 5.0,
                GrammarRangeScore = 5.0,
                TotalScore = 5.0,
                Strengths = "AI grading temporarily unavailable",
                Weaknesses = "Using fallback grading",
                Suggestions = "Please try again later",
                DetailedFeedback = new List<WritingDetailedFeedback>()
            }).ToList();
        }

        // 5. Tạo chi tiết và tính điểm
        var details = new List<SubmissionAnswerDetailDto>();
        double totalScore = 0;

        for (int i = 0; i < questionList.Count && i < aiResults.Count; i++)
        {
            var question = questionList[i];
            var aiResult = aiResults[i];
            var essayText = essayTexts[question.Id];
            var wordCount = wordCounts[question.Id];

            totalScore += aiResult.TotalScore;

            details.Add(new SubmissionAnswerDetailDto
            {
                QuestionId = question.Id,
                QuestionText = question.PromptText,
                OrderNumber = question.OrderNumber,
                UserAnswer = essayText,
                CorrectAnswer = null,
                IsCorrect = aiResult.TotalScore >= 5.0,
                AiScore = aiResult.TotalScore,
                AiFeedback = FormatAiFeedback(aiResult, wordCount),
                Explanation = null
            });
        }

        // 6. Tính điểm trung bình
        var averageScore = Math.Round(totalScore / questionList.Count, 1);
        averageScore = Math.Clamp(averageScore, 0, 10);

        // 7. Tạo Submission
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExerciseId = request.ExerciseId,
            SkillType = 2,
            TotalScore = (short)Math.Round(averageScore),
            TotalQuestions = questionList.Count,
            CorrectCount = 0,
            TimeSpentSeconds = request.TimeSpentSeconds,
            AnswerJson = JsonSerializer.Serialize(request.Answers),
            EssayText = JsonSerializer.Serialize(new
            {
                task1 = request.Task1Essay,
                task2 = request.Task2Essay,
                answers = request.Answers
            }),
            AiFeedback = JsonSerializer.Serialize(new
            {
                Summary = "Writing graded by AI",
                Results = aiResults.Select(r => new
                {
                    r.TaskResponseScore,
                    r.CoherenceCohesionScore,
                    r.LexicalResourceScore,
                    r.GrammarRangeScore,
                    r.TotalScore,
                    r.Strengths,
                    r.Weaknesses,
                    r.Suggestions,
                    DetailedFeedback = r.DetailedFeedback // ✅ THÊM: Lưu detailed feedback
                })
            }),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Submissions.AddAsync(submission);

        // 8. Lưu SubmissionDetail cho từng câu
        foreach (var question in questionList)
        {
            string essayText = essayTexts[question.Id];
            var aiResult = aiResults.FirstOrDefault();
            var wordCount = wordCounts[question.Id];

            var detail = new SubmissionDetail
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                QuestionId = question.Id,
                QuestionType = "Writing",
                OrderNumber = question.OrderNumber,
                UserAnswer = essayText,
                CorrectAnswer = null,
                IsCorrect = aiResult?.TotalScore >= 5.0,
                PointEarned = (short)Math.Round(aiResult?.TotalScore ?? 0),
                AiScore = aiResult?.TotalScore ?? 0,
                AiFeedback = aiResult != null ? FormatAiFeedback(aiResult, wordCount) : string.Empty,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _unitOfWork.SubmissionDetails.AddAsync(detail);
        }

        // 9. Cập nhật Full Test Session
        if (request.SessionId.HasValue && request.SessionId.Value != Guid.Empty)
        {
            var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId.Value);
            if (session != null)
            {
                session.WritingSubmissionId = submission.Id;
                session.WritingTimeSpent = request.TimeSpentSeconds;
                await _unitOfWork.FullTestSessions.UpdateAsync(session);
                Console.WriteLine($"✅ Updated session {session.Id} with WritingSubmissionId: {submission.Id}");
            }
        }

        // 10. Cập nhật AttemptCount
        exercise.AttemptCount++;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        // 11. Trả về kết quả
        return new SubmissionDetailDto
        {
            Id = submission.Id,
            ExerciseId = submission.ExerciseId,
            ExerciseTitle = exercise.Title,
            Skill = 2,
            SkillName = "Writing",
            TotalScore = averageScore,
            TotalQuestions = questionList.Count,
            CorrectCount = 0,
            SubmittedAt = submission.SubmittedAt,
            Details = details,
            AiFeedback = new
            {
                Summary = "Writing graded by AI",
                Results = aiResults.Select(r => new
                {
                    r.TaskResponseScore,
                    r.CoherenceCohesionScore,
                    r.LexicalResourceScore,
                    r.GrammarRangeScore,
                    r.TotalScore,
                    r.Strengths,
                    r.Weaknesses,
                    r.Suggestions,
                    DetailedFeedback = r.DetailedFeedback // ✅ THÊM: Lưu detailed feedback
                })
            }
        };
    }

    private static string GetEssayText(SubmitWritingCommand request, WritingQuestion question)
    {
        if (request.Answers != null && request.Answers.TryGetValue(question.Id, out var essay))
            return essay;

        if (question.OrderNumber == 1 && !string.IsNullOrEmpty(request.Task1Essay))
            return request.Task1Essay;

        if (question.OrderNumber == 2 && !string.IsNullOrEmpty(request.Task2Essay))
            return request.Task2Essay;

        return string.Empty;
    }

    // ============================================================
    // FORMAT AI FEEDBACK - HIỂN THỊ CHI TIẾT
    // ============================================================

    private static string FormatAiFeedback(WritingGradeResult result, int wordCount)
    {
        if (result == null) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"📊 Điểm tổng: {result.TotalScore}/10");
        sb.AppendLine($"📝 Số từ: {wordCount}");
        sb.AppendLine();
        sb.AppendLine("📌 Điểm từng tiêu chí:");
        sb.AppendLine($"   • Task Response: {result.TaskResponseScore}/10");
        sb.AppendLine($"   • Coherence & Cohesion: {result.CoherenceCohesionScore}/10");
        sb.AppendLine($"   • Lexical Resource: {result.LexicalResourceScore}/10");
        sb.AppendLine($"   • Grammatical Range: {result.GrammarRangeScore}/10");
        sb.AppendLine();
        sb.AppendLine($"✅ Điểm mạnh:\n{result.Strengths}");
        sb.AppendLine();
        sb.AppendLine($"⚠️ Điểm yếu:\n{result.Weaknesses}");
        sb.AppendLine();
        sb.AppendLine($"💡 Gợi ý cải thiện:\n{result.Suggestions}");

        // ✅ THÊM: Detailed Feedback
        if (result.DetailedFeedback != null && result.DetailedFeedback.Any())
        {
            sb.AppendLine();
            sb.AppendLine("📝 Phân tích chi tiết từng lỗi:");
            foreach (var feedback in result.DetailedFeedback)
            {
                sb.AppendLine($"   • Vấn đề: {feedback.Issue}");
                sb.AppendLine($"     Câu sai: \"{feedback.Sentence}\"");
                sb.AppendLine($"     Sửa: \"{feedback.Suggestion}\"");
            }
        }

        return sb.ToString();
    }
}