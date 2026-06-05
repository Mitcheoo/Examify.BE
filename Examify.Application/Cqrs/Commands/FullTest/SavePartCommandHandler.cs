using MediatR;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.FullTest;
using System.Text.Json;

namespace Examify.Application.Cqrs.Commands.FullTest;

public class SavePartCommandHandler : IRequestHandler<SavePartCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIGradingService _aiGradingService;

    public SavePartCommandHandler(IUnitOfWork unitOfWork, IAIGradingService aiGradingService)
    {
        _unitOfWork = unitOfWork;
        _aiGradingService = aiGradingService;
    }

    public async Task<bool> Handle(SavePartCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId);
        if (session is null)
            throw new FullTestException("Session not found");

        Submission? submission = null;

        switch (request.PartNumber)
        {
            case 1:
                submission = await ProcessReading(request, session);
                session.ReadingSubmissionId = submission.Id;
                session.ReadingTimeSpent = request.TimeSpentSeconds;
                break;
            case 2:
                submission = await ProcessListening(request, session);
                session.ListeningSubmissionId = submission.Id;
                session.ListeningTimeSpent = request.TimeSpentSeconds;
                break;
            case 3:
                submission = await ProcessWriting(request, session);
                session.WritingSubmissionId = submission.Id;
                session.WritingTimeSpent = request.TimeSpentSeconds;
                break;
            case 4:
                submission = await ProcessSpeaking(request, session);
                session.SpeakingSubmissionId = submission.Id;
                session.SpeakingTimeSpent = request.TimeSpentSeconds;
                break;
        }

        if (request.PartNumber < 4)
        {
            session.CurrentPart = request.PartNumber + 1;
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task<Submission> ProcessReading(SavePartCommand request, FullTestSession session)
    {
        var questions = await _unitOfWork.ReadingQuestions
            .FindAsync(q => q.ExerciseId == session.ReadingExerciseId);

        var questionList = questions.ToList();
        var correctCount = 0;
        var details = new List<SubmissionDetail>();

        foreach (var q in questionList)
        {
            request.Answers.TryGetValue(q.Id, out var userAnswer);
            var isCorrect = string.Equals(userAnswer, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            if (isCorrect) correctCount++;

            details.Add(new SubmissionDetail
            {
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                UserAnswer = userAnswer ?? "",
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
            TimeSpentSeconds = request.TimeSpentSeconds,
            ResultJson = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        return submission;
    }

    private async Task<Submission> ProcessListening(SavePartCommand request, FullTestSession session)
    {
        var questions = await _unitOfWork.ListeningQuestions
            .FindAsync(q => q.ExerciseId == session.ListeningExerciseId);

        var questionList = questions.ToList();
        var correctCount = 0;
        var details = new List<SubmissionDetail>();

        foreach (var q in questionList)
        {
            request.Answers.TryGetValue(q.Id, out var userAnswer);
            var isCorrect = userAnswer == q.CorrectAnswer;
            if (isCorrect) correctCount++;

            details.Add(new SubmissionDetail
            {
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                UserAnswer = userAnswer ?? "",
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
            TimeSpentSeconds = request.TimeSpentSeconds,
            ResultJson = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        return submission;
    }

    private async Task<Submission> ProcessWriting(SavePartCommand request, FullTestSession session)
    {
        var questions = await _unitOfWork.WritingQuestions
            .FindAsync(q => q.ExerciseId == session.WritingExerciseId);

        var question = questions.FirstOrDefault();
        var aiResult = await _aiGradingService.GradeWritingAsync(request.EssayText ?? "", question?.PromptText ?? "");

        var submission = new Submission
        {
            UserId = session.UserId,
            ExerciseId = session.WritingExerciseId!.Value,
            SkillType = 2,
            TotalScore = (short)Math.Round(aiResult.TotalScore, MidpointRounding.AwayFromZero),
            TotalQuestions = 1,
            CorrectCount = 0,
            TimeSpentSeconds = request.TimeSpentSeconds,
            EssayText = request.EssayText,
            AiFeedback = JsonSerializer.Serialize(aiResult),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        return submission;
    }

    private async Task<Submission> ProcessSpeaking(SavePartCommand request, FullTestSession session)
    {
        var transcript = request.Answers.TryGetValue(Guid.Empty, out var t) ? t : "";

        var questions = await _unitOfWork.SpeakingQuestions
            .FindAsync(q => q.ExerciseId == session.SpeakingExerciseId);

        var questionList = questions.ToList();
        var totalScore = 0.0;
        var aiResults = new List<SpeakingGradeResult>();

        foreach (var q in questionList)
        {
            var aiResult = await _aiGradingService.GradeSpeakingAsync(transcript, q.QuestionText);
            totalScore += aiResult.TotalScore;
            aiResults.Add(aiResult);
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
            TimeSpentSeconds = request.TimeSpentSeconds,
            AudioUrl = request.AudioUrl,
            Transcript = transcript,
            AiFeedback = JsonSerializer.Serialize(aiResults),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        return submission;
    }
}