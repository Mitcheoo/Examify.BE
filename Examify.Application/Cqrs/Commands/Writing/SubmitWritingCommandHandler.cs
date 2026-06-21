// Examify.Application/Cqrs/Commands/Writing/SubmitWritingCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Submissions;
using System.Text.Json;

namespace Examify.Application.Cqrs.Commands.Writing;

public class SubmitWritingCommandHandler : IRequestHandler<SubmitWritingCommand, SubmissionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIGradingService _aiGradingService;

    public SubmitWritingCommandHandler(IUnitOfWork unitOfWork, IAIGradingService aiGradingService)
    {
        _unitOfWork = unitOfWork;
        _aiGradingService = aiGradingService;
    }

    public async Task<SubmissionDetailDto> Handle(SubmitWritingCommand request, CancellationToken cancellationToken)
    {
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

        foreach (var question in questionList)
        {
            string essayText = GetEssayText(request, question);
            essayTexts[question.Id] = essayText;
            essaysForAI.Add((essayText, question.PromptText));
        }

        // 4. Gọi AI chấm BATCH
        var aiResults = await _aiGradingService.GradeWritingBatchAsync(essaysForAI);

        // ✅ THÊM: KIỂM TRA AI RESULTS RỖNG
        if (aiResults == null || aiResults.Count == 0)
        {
            // Nếu AI lỗi, dùng fallback grading
            aiResults = questionList.Select(q => new WritingGradeResult
            {
                TaskResponseScore = 5.0,
                CoherenceCohesionScore = 5.0,
                LexicalResourceScore = 5.0,
                GrammarRangeScore = 5.0,
                TotalScore = 5.0,
                Strengths = "AI grading temporarily unavailable",
                Weaknesses = "Using fallback grading",
                Suggestions = "Please try again later"
            }).ToList();
        }

        // 5. Tạo chi tiết và tính điểm - ✅ Dùng SubmissionAnswerDetailDto
        var details = new List<SubmissionAnswerDetailDto>();
        double totalScore = 0;

        for (int i = 0; i < questionList.Count && i < aiResults.Count; i++)
        {
            var question = questionList[i];
            var aiResult = aiResults[i];
            var essayText = essayTexts[question.Id];

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
                AiFeedback = FormatAiFeedback(aiResult),
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
            UserId = request.UserId,
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
            AiFeedback = JsonSerializer.Serialize(aiResults),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Submissions.AddAsync(submission);

        // 8. Cập nhật AttemptCount
        exercise.AttemptCount++;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        // 9. Lưu SubmissionDetail cho từng câu
        foreach (var question in questionList)
        {
            string essayText = essayTexts[question.Id];
            var aiResult = aiResults.FirstOrDefault();

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
                AiFeedback = aiResult != null ? FormatAiFeedback(aiResult) : string.Empty,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _unitOfWork.SubmissionDetails.AddAsync(detail);
        }

        await _unitOfWork.SaveChangesAsync();

        // 10. Trả về kết quả - ✅ Dùng SubmissionDetailDto
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
                    r.Suggestions
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

    private static string FormatAiFeedback(WritingGradeResult result)
    {
        if (result == null) return string.Empty;
        return $"Điểm: {result.TotalScore}/10\n" +
               $"Điểm mạnh: {result.Strengths}\n" +
               $"Điểm yếu: {result.Weaknesses}\n" +
               $"Gợi ý: {result.Suggestions}";
    }
}