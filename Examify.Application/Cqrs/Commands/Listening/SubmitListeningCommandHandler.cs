// 📁 Examify.Application/Cqrs/Commands/Listening/SubmitListeningCommandHandler.cs

using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Submissions;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Examify.Application.Cqrs.Commands.Listening;

public class SubmitListeningCommandHandler : IRequestHandler<SubmitListeningCommand, SubmissionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubmitListeningCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SubmissionDetailDto> Handle(SubmitListeningCommand request, CancellationToken cancellationToken)
    {
        // ✅ LẤY USER ID TỪ TOKEN
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new Exception("User not authenticated");
        var userId = Guid.Parse(userIdClaim);
        Console.WriteLine($"👤 User ID from token: {userId}");

        // 1. Lấy câu hỏi Listening
        var questions = await _unitOfWork.ListeningQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted);

        var questionList = questions.OrderBy(q => q.OrderNumber).ToList();

        if (questionList.Count == 0)
            throw new NotFoundException("No questions found for this exercise");

        // 2. Lấy thông tin Exercise
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        // 3. Chấm điểm từng câu
        int correctCount = 0;
        var details = new List<SubmissionAnswerDetailDto>();

        foreach (var q in questionList)
        {
            var userAnswer = request.Answers.TryGetValue(q.Id, out var answer) ? answer : string.Empty;
            var isCorrect = !string.IsNullOrEmpty(userAnswer) && userAnswer == q.CorrectAnswer;

            if (isCorrect) correctCount++;

            details.Add(new SubmissionAnswerDetailDto
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                OrderNumber = q.OrderNumber,
                UserAnswer = userAnswer,
                CorrectAnswer = q.CorrectAnswer,
                IsCorrect = isCorrect,
                Explanation = q.Explanation,
                AiScore = null,
                AiFeedback = null
            });
        }

        // 4. Tính điểm
        var totalScore = Math.Round((double)correctCount / questionList.Count * 10, 1);

        // 5. Tạo Submission
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            UserId = userId,  // ✅ DÙNG USER ID TỪ TOKEN
            ExerciseId = request.ExerciseId,
            SkillType = 1,
            TotalScore = (short)totalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = correctCount,
            TimeSpentSeconds = request.TimeSpentSeconds,
            ResultJson = JsonSerializer.Serialize(request.Answers),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Submissions.AddAsync(submission);

        // 6. Lưu SubmissionDetail cho từng câu
        foreach (var q in questionList)
        {
            var userAnswer = request.Answers.TryGetValue(q.Id, out var answer) ? answer : string.Empty;
            var isCorrect = !string.IsNullOrEmpty(userAnswer) && userAnswer == q.CorrectAnswer;

            var detail = new SubmissionDetail
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                QuestionId = q.Id,
                QuestionType = "Listening",
                OrderNumber = q.OrderNumber,
                UserAnswer = userAnswer,
                CorrectAnswer = q.CorrectAnswer,
                IsCorrect = isCorrect,
                PointEarned = isCorrect ? (short)1 : (short)0,
                AiScore = null,
                AiFeedback = null,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.SubmissionDetails.AddAsync(detail);
        }

        // 7. Cập nhật Full Test Session
        if (request.SessionId.HasValue && request.SessionId.Value != Guid.Empty)
        {
            var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId.Value);
            if (session != null)
            {
                session.ListeningSubmissionId = submission.Id;
                session.ListeningTimeSpent = request.TimeSpentSeconds;
                await _unitOfWork.FullTestSessions.UpdateAsync(session);
                Console.WriteLine($"✅ Updated session {session.Id} with ListeningSubmissionId: {submission.Id}");
            }
        }

        // 8. Cập nhật AttemptCount
        exercise.AttemptCount++;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        // 9. Trả về kết quả
        return new SubmissionDetailDto
        {
            Id = submission.Id,
            ExerciseId = submission.ExerciseId,
            ExerciseTitle = exercise.Title,
            Skill = 1,
            SkillName = "Listening",
            TotalScore = totalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = correctCount,
            SubmittedAt = submission.SubmittedAt,
            Details = details,
            AiFeedback = null
        };
    }
}