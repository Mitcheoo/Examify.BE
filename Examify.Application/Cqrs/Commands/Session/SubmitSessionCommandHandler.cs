// Examify.Application/Cqrs/Commands/Session/SubmitSessionCommandHandler.cs
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
        var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId);
        if (session is null)
            throw new FullTestException("Session not found");

        if (session.Status == 1)
            throw new BadRequestException("Session already completed");

        // Lấy tất cả answers tạm
        var draftAnswers = await _unitOfWork.SessionAnswers
            .FindAsync(a => a.SessionId == request.SessionId && !a.IsSubmitted);

        var answerList = draftAnswers.ToList();

        if (answerList.Count == 0)
            throw new BadRequestException("No answers found to submit");

        // Đánh dấu đã submit
        foreach (var answer in answerList)
        {
            answer.IsSubmitted = true;
            await _unitOfWork.SessionAnswers.UpdateAsync(answer);
        }

        // Nhóm answers theo kỹ năng
        var readingAnswers = answerList.Where(a => a.SkillType == 0).ToList();
        var listeningAnswers = answerList.Where(a => a.SkillType == 1).ToList();
        var writingAnswers = answerList.Where(a => a.SkillType == 2).ToList();
        var speakingAnswers = answerList.Where(a => a.SkillType == 3).ToList();

        // Xử lý từng kỹ năng và tạo submissions
        short readingScore = 0, listeningScore = 0, writingScore = 0, speakingScore = 0;

        // Reading
        if (readingAnswers.Any())
        {
            readingScore = await ProcessReading(session, readingAnswers);
        }

        // Listening
        if (listeningAnswers.Any())
        {
            listeningScore = await ProcessListening(session, listeningAnswers);
        }

        // Writing
        if (writingAnswers.Any())
        {
            writingScore = await ProcessWriting(session, writingAnswers);
        }

        // Speaking
        if (speakingAnswers.Any())
        {
            speakingScore = await ProcessSpeaking(session, speakingAnswers);
        }

        // Tính tổng điểm
        var scores = new double[]
        {
            readingScore,
            listeningScore,
            writingScore,
            speakingScore
        };
        var totalScore = (short)Math.Round(scores.Where(s => s > 0).DefaultIfEmpty(0).Average());

        // Cập nhật session
        session.TotalScore = totalScore;
        session.Status = 1;
        session.EndTime = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        // Cập nhật Leaderboard
        await UpdateLeaderboard(session.UserId, totalScore);

        var totalTime = session.ReadingTimeSpent + session.ListeningTimeSpent +
                        session.WritingTimeSpent + session.SpeakingTimeSpent;

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

    private async Task<short> ProcessReading(FullTestSession session, List<SessionAnswer> answers)
    {
        var questions = await _unitOfWork.ReadingQuestions
            .FindAsync(q => q.ExerciseId == session.ReadingExerciseId);

        var questionList = questions.ToList();
        var correctCount = 0;
        var details = new List<SubmissionDetail>();

        foreach (var q in questionList)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var isCorrect = string.Equals(answer?.UserAnswer, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            if (isCorrect) correctCount++;

            details.Add(new SubmissionDetail
            {
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                UserAnswer = answer?.UserAnswer ?? "",
                CorrectAnswer = q.CorrectAnswer,
                IsCorrect = isCorrect,
                PointEarned = isCorrect ? (short)1 : (short)0
            });
        }

        var totalScore = (short)Math.Round((double)correctCount / questionList.Count * 10, MidpointRounding.AwayFromZero);

        var submission = new Submission
        {
            UserId = session.UserId,
            ExerciseId = session.ReadingExerciseId!.Value,
            SkillType = 0,
            TotalScore = totalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = correctCount,
            TimeSpentSeconds = session.ReadingTimeSpent,
            ResultJson = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        session.ReadingSubmissionId = submission.Id;

        return totalScore;
    }

    private async Task<short> ProcessListening(FullTestSession session, List<SessionAnswer> answers)
    {
        var questions = await _unitOfWork.ListeningQuestions
            .FindAsync(q => q.ExerciseId == session.ListeningExerciseId);

        var questionList = questions.ToList();
        var correctCount = 0;
        var details = new List<SubmissionDetail>();

        foreach (var q in questionList)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var isCorrect = answer?.UserAnswer == q.CorrectAnswer;
            if (isCorrect) correctCount++;

            details.Add(new SubmissionDetail
            {
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                UserAnswer = answer?.UserAnswer ?? "",
                CorrectAnswer = q.CorrectAnswer,
                IsCorrect = isCorrect,
                PointEarned = isCorrect ? (short)1 : (short)0
            });
        }

        var totalScore = (short)Math.Round((double)correctCount / questionList.Count * 10, MidpointRounding.AwayFromZero);

        var submission = new Submission
        {
            UserId = session.UserId,
            ExerciseId = session.ListeningExerciseId!.Value,
            SkillType = 1,
            TotalScore = totalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = correctCount,
            TimeSpentSeconds = session.ListeningTimeSpent,
            ResultJson = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        session.ListeningSubmissionId = submission.Id;

        return totalScore;
    }

    private async Task<short> ProcessWriting(FullTestSession session, List<SessionAnswer> answers)
    {
        var questions = await _unitOfWork.WritingQuestions
            .FindAsync(q => q.ExerciseId == session.WritingExerciseId);

        var question = questions.FirstOrDefault();
        if (question is null)
            return 0;

        var answer = answers.FirstOrDefault(a => a.QuestionId == question.Id);
        var essayText = answer?.UserAnswer ?? "";

        var aiResult = await _aiGradingService.GradeWritingAsync(essayText, question.PromptText);

        var submission = new Submission
        {
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
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        session.WritingSubmissionId = submission.Id;

        return submission.TotalScore;
    }

    private async Task<short> ProcessSpeaking(FullTestSession session, List<SessionAnswer> answers)
    {
        var questions = await _unitOfWork.SpeakingQuestions
            .FindAsync(q => q.ExerciseId == session.SpeakingExerciseId);

        var questionList = questions.ToList();
        if (questionList.Count == 0) return 0;

        double totalScore = 0;
        var details = new List<SubmissionDetail>();

        foreach (var q in questionList)
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var transcript = answer?.Transcript ?? answer?.UserAnswer ?? "";

            var aiResult = await _aiGradingService.GradeSpeakingContentAsync(transcript, q.QuestionText);
            totalScore += aiResult.TotalScore;

            details.Add(new SubmissionDetail
            {
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                UserAnswer = transcript,
                IsCorrect = false,
                AiScore = aiResult.TotalScore,
                AiFeedback = $"{aiResult.Strengths}\n{aiResult.Weaknesses}\n{aiResult.Suggestions}"
            });
        }

        var averageScore = (short)Math.Round(totalScore / questionList.Count, MidpointRounding.AwayFromZero);

        var submission = new Submission
        {
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
            IsGraded = true
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
                UserId = userId,
                SkillType = 4,
                TotalScore = totalScore,
                TotalAttempts = 1,
                AverageScore = totalScore,
                LastUpdated = DateTime.UtcNow
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