// 📁 Examify.Application/Cqrs/Commands/Session/SubmitSessionCommandHandler.cs

using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.FullTest;
using System.Text.Json;

namespace Examify.Application.Cqrs.Commands.Session;

public sealed class SubmitSessionCommandHandler : IRequestHandler<SubmitSessionCommand, FullTestResultResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIGradingService _aiGradingService;

    public SubmitSessionCommandHandler(IUnitOfWork unitOfWork, IAIGradingService aiGradingService)
    {
        _unitOfWork = unitOfWork;
        _aiGradingService = aiGradingService;
    }

    public async Task<FullTestResultResponse> Handle(SubmitSessionCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"========== SUBMIT SESSION ==========");
        Console.WriteLine($"📌 SessionId: {request.SessionId}");

        // 1. Kiểm tra session tồn tại
        var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId);
        if (session is null)
            throw new FullTestException("Session not found");

        if (session.Status == 1)
            throw new BadRequestException("Session already completed");

        Console.WriteLine($"✅ Session found: {session.Id}");
        Console.WriteLine($"   UserId: {session.UserId}");
        Console.WriteLine($"   FullTestId: {session.FullTestId}");

        // 2. Lấy tất cả draft answers chưa submit
        var draftAnswers = await _unitOfWork.SessionAnswers
            .FindAsync(a => a.SessionId == request.SessionId && !a.IsSubmitted && !a.IsDeleted);

        var answerList = draftAnswers.ToList();
        Console.WriteLine($"📦 Found {answerList.Count} draft answers");

        if (answerList.Count == 0)
            throw new BadRequestException("No answers found to submit");

        // 3. Đánh dấu đã submit (trước khi xử lý)
        foreach (var answer in answerList)
        {
            answer.IsSubmitted = true;
            await _unitOfWork.SessionAnswers.UpdateAsync(answer);
        }
        await _unitOfWork.SaveChangesAsync();
        Console.WriteLine("✅ Marked all answers as submitted");

        // 4. Nhóm answers theo kỹ năng
        var readingAnswers = answerList.Where(a => a.SkillType == 0).ToList();
        var listeningAnswers = answerList.Where(a => a.SkillType == 1).ToList();
        var writingAnswers = answerList.Where(a => a.SkillType == 2).ToList();
        var speakingAnswers = answerList.Where(a => a.SkillType == 3).ToList();

        Console.WriteLine($"📊 Reading: {readingAnswers.Count}, Listening: {listeningAnswers.Count}, Writing: {writingAnswers.Count}, Speaking: {speakingAnswers.Count}");

        // 5. Xử lý từng kỹ năng và tạo submissions
        short readingScore = 0, listeningScore = 0, writingScore = 0, speakingScore = 0;

        // Reading
        if (readingAnswers.Any())
        {
            readingScore = await ProcessReading(session, readingAnswers);
            Console.WriteLine($"✅ Reading score: {readingScore}");
        }

        // Listening
        if (listeningAnswers.Any())
        {
            listeningScore = await ProcessListening(session, listeningAnswers);
            Console.WriteLine($"✅ Listening score: {listeningScore}");
        }

        // Writing
        if (writingAnswers.Any())
        {
            writingScore = await ProcessWriting(session, writingAnswers);
            Console.WriteLine($"✅ Writing score: {writingScore}");
        }

        // Speaking
        if (speakingAnswers.Any())
        {
            speakingScore = await ProcessSpeaking(session, speakingAnswers);
            Console.WriteLine($"✅ Speaking score: {speakingScore}");
        }

        // 6. Tính tổng điểm
        var scores = new double[]
        {
            readingScore,
            listeningScore,
            writingScore,
            speakingScore
        };
        var totalScore = (short)Math.Round(scores.Where(s => s > 0).DefaultIfEmpty(0).Average());
        Console.WriteLine($"📊 Total score: {totalScore}");

        // 7. Cập nhật session
        session.TotalScore = totalScore;
        session.Status = 1;
        session.EndTime = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        Console.WriteLine("✅ Session updated");

        // 8. ✅ XÓA TẤT CẢ DRAFT ANSWERS SAU KHI SUBMIT (HARD DELETE)
        Console.WriteLine($"🗑️ Deleting all draft answers for session: {session.Id}");

        var allDraftAnswers = await _unitOfWork.SessionAnswers
            .FindAsync(a => a.SessionId == request.SessionId);

        foreach (var answer in allDraftAnswers)
        {
            // HARD DELETE (xóa vĩnh viễn)
            await _unitOfWork.SessionAnswers.DeleteAsync(answer);
            Console.WriteLine($"   ✅ Deleted answer for question: {answer.QuestionId}");
        }

        await _unitOfWork.SaveChangesAsync();
        Console.WriteLine($"✅ All draft answers deleted for session: {session.Id}");

        // 9. Cập nhật Leaderboard
        await UpdateLeaderboard(session.UserId, totalScore);
        Console.WriteLine("✅ Leaderboard updated");

        // 10. Tính tổng thời gian
        var totalTime = session.ReadingTimeSpent + session.ListeningTimeSpent +
                        session.WritingTimeSpent + session.SpeakingTimeSpent;

        Console.WriteLine($"⏱️ Total time: {totalTime} seconds");
        Console.WriteLine("========== SUBMIT COMPLETED ==========");

        return new FullTestResultResponse
        {
            SessionId = session.Id,
            TotalScore = totalScore,
            ReadingScore = readingScore,
            ListeningScore = listeningScore,
            WritingScore = writingScore,
            SpeakingScore = speakingScore,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            TotalTimeSpentSeconds = totalTime
        };
    }

    // ============================================================
    // PRIVATE METHODS
    // ============================================================

    private async Task<short> ProcessReading(FullTestSession session, List<SessionAnswer> answers)
    {
        var questions = await _unitOfWork.ReadingQuestions
            .FindAsync(q => q.ExerciseId == session.ReadingExerciseId && !q.IsDeleted);

        var questionList = questions.ToList();
        if (questionList.Count == 0) return 0;

        var correctCount = 0;
        var details = new List<SubmissionDetail>();

        foreach (var q in questionList)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var isCorrect = string.Equals(answer?.UserAnswer?.Trim(), q.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
            if (isCorrect) correctCount++;

            details.Add(new SubmissionDetail
            {
                Id = Guid.NewGuid(),
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                UserAnswer = answer?.UserAnswer ?? "",
                CorrectAnswer = q.CorrectAnswer,
                IsCorrect = isCorrect,
                PointEarned = isCorrect ? (short)1 : (short)0,
                CreatedAt = DateTime.UtcNow
            });
        }

        var totalScore = (short)Math.Round((double)correctCount / questionList.Count * 10, MidpointRounding.AwayFromZero);

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            UserId = session.UserId,
            ExerciseId = session.ReadingExerciseId!.Value,
            SkillType = 0,
            TotalScore = totalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = correctCount,
            TimeSpentSeconds = session.ReadingTimeSpent,
            ResultJson = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        session.ReadingSubmissionId = submission.Id;

        return totalScore;
    }

    private async Task<short> ProcessListening(FullTestSession session, List<SessionAnswer> answers)
    {
        var questions = await _unitOfWork.ListeningQuestions
            .FindAsync(q => q.ExerciseId == session.ListeningExerciseId && !q.IsDeleted);

        var questionList = questions.ToList();
        if (questionList.Count == 0) return 0;

        var correctCount = 0;
        var details = new List<SubmissionDetail>();

        foreach (var q in questionList)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var isCorrect = string.Equals(answer?.UserAnswer?.Trim(), q.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
            if (isCorrect) correctCount++;

            details.Add(new SubmissionDetail
            {
                Id = Guid.NewGuid(),
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                UserAnswer = answer?.UserAnswer ?? "",
                CorrectAnswer = q.CorrectAnswer,
                IsCorrect = isCorrect,
                PointEarned = isCorrect ? (short)1 : (short)0,
                CreatedAt = DateTime.UtcNow
            });
        }

        var totalScore = (short)Math.Round((double)correctCount / questionList.Count * 10, MidpointRounding.AwayFromZero);

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            UserId = session.UserId,
            ExerciseId = session.ListeningExerciseId!.Value,
            SkillType = 1,
            TotalScore = totalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = correctCount,
            TimeSpentSeconds = session.ListeningTimeSpent,
            ResultJson = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        session.ListeningSubmissionId = submission.Id;

        return totalScore;
    }

    private async Task<short> ProcessWriting(FullTestSession session, List<SessionAnswer> answers)
    {
        var questions = await _unitOfWork.WritingQuestions
            .FindAsync(q => q.ExerciseId == session.WritingExerciseId && !q.IsDeleted);

        var question = questions.FirstOrDefault();
        if (question is null) return 0;

        var answer = answers.FirstOrDefault(a => a.QuestionId == question.Id);
        var essayText = answer?.UserAnswer ?? "";

        // Nếu không có nội dung, trả về 0
        if (string.IsNullOrWhiteSpace(essayText))
            return 0;

        var aiResult = await _aiGradingService.GradeWritingAsync(essayText, question.PromptText);

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            UserId = session.UserId,
            ExerciseId = session.WritingExerciseId!.Value,
            SkillType = 2,
            TotalScore = (short)Math.Round(aiResult.TotalScore, MidpointRounding.AwayFromZero),
            TotalQuestions = 1,
            CorrectCount = 0,
            TimeSpentSeconds = session.WritingTimeSpent,
            EssayText = essayText,
            AiFeedback = JsonSerializer.Serialize(aiResult),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        session.WritingSubmissionId = submission.Id;

        return submission.TotalScore;
    }

    private async Task<short> ProcessSpeaking(FullTestSession session, List<SessionAnswer> answers)
    {
        var questions = await _unitOfWork.SpeakingQuestions
            .FindAsync(q => q.ExerciseId == session.SpeakingExerciseId && !q.IsDeleted);

        var questionList = questions.ToList();
        if (questionList.Count == 0) return 0;

        double totalScore = 0;
        var details = new List<SubmissionDetail>();

        foreach (var q in questionList)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var transcript = answer?.Transcript ?? answer?.UserAnswer ?? "";

            // Nếu không có transcript, đánh giá thấp
            if (string.IsNullOrWhiteSpace(transcript))
            {
                details.Add(new SubmissionDetail
                {
                    Id = Guid.NewGuid(),
                    QuestionId = q.Id,
                    OrderNumber = q.OrderNumber,
                    UserAnswer = "",
                    IsCorrect = false,
                    AiScore = 0,
                    AiFeedback = "No audio or transcript provided",
                    CreatedAt = DateTime.UtcNow
                });
                continue;
            }

            var aiResult = await _aiGradingService.GradeSpeakingContentAsync(transcript, q.QuestionText);
            totalScore += aiResult.TotalScore;

            details.Add(new SubmissionDetail
            {
                Id = Guid.NewGuid(),
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                UserAnswer = transcript,
                IsCorrect = false,
                AiScore = aiResult.TotalScore,
                AiFeedback = $"{aiResult.Strengths}\n{aiResult.Weaknesses}\n{aiResult.Suggestions}",
                CreatedAt = DateTime.UtcNow
            });
        }

        var averageScore = (short)Math.Round(totalScore / questionList.Count, MidpointRounding.AwayFromZero);

        // Nếu không có câu nào có transcript, trả về 0
        if (details.All(d => string.IsNullOrWhiteSpace(d.UserAnswer)))
            return 0;

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            UserId = session.UserId,
            ExerciseId = session.SpeakingExerciseId!.Value,
            SkillType = 3,
            TotalScore = averageScore,
            TotalQuestions = questionList.Count,
            CorrectCount = 0,
            TimeSpentSeconds = session.SpeakingTimeSpent,
            Transcript = string.Join("\n", details.Select(d => d.UserAnswer ?? "")),
            AiFeedback = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        session.SpeakingSubmissionId = submission.Id;

        return averageScore;
    }

    private async Task UpdateLeaderboard(Guid userId, short totalScore)
    {
        var leaderboard = await _unitOfWork.Leaderboards
            .FindAsync(l => l.UserId == userId && l.SkillType == 4);

        var entry = leaderboard.FirstOrDefault();
        if (entry is null)
        {
            entry = new Leaderboard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SkillType = 4,
                TotalScore = totalScore,
                TotalAttempts = 1,
                AverageScore = totalScore,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Leaderboards.AddAsync(entry);
        }
        else
        {
            entry.TotalScore = totalScore;
            entry.TotalAttempts++;
            entry.AverageScore = (entry.AverageScore * (entry.TotalAttempts - 1) + totalScore) / entry.TotalAttempts;
            entry.LastUpdated = DateTime.UtcNow;
            await _unitOfWork.Leaderboards.UpdateAsync(entry);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}