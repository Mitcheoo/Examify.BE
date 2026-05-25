// Examify.Application/Cqrs/Commands/Speaking/SubmitSpeakingCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Submissions;
using System.Text.Json;

namespace Examify.Application.Cqrs.Commands.Speaking;

public class SubmitSpeakingCommandHandler : IRequestHandler<SubmitSpeakingCommand, SubmissionResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIGradingService _aiGradingService;

    public SubmitSpeakingCommandHandler(IUnitOfWork unitOfWork, IAIGradingService aiGradingService)
    {
        _unitOfWork = unitOfWork;
        _aiGradingService = aiGradingService;
    }

    public async Task<SubmissionResultDto> Handle(SubmitSpeakingCommand request, CancellationToken cancellationToken)
    {
        var questions = await _unitOfWork.SpeakingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId);

        var questionList = questions.ToList();

        if (questionList.Count == 0)
            throw new NotFoundException("Speaking questions not found");

        var details = new List<SubmissionDetailDto>();
        double totalScore = 0;

        foreach (var q in questionList)
        {
            string transcript = string.Empty;

            if (request.Answers.TryGetValue(q.Id, out var answer))
            {
                transcript = answer;
            }

            var aiResult = await _aiGradingService.GradeSpeakingAsync(transcript, q.QuestionText);

            details.Add(new SubmissionDetailDto
            {
                QuestionId = q.Id,
                OrderNumber = q.OrderNumber,
                QuestionText = q.QuestionText,
                UserAnswer = transcript,
                IsCorrect = false,
                Explanation = $"{aiResult.Strengths}\n{aiResult.Weaknesses}"
            });

            totalScore += aiResult.TotalScore;
        }

        var averageScore = (short)Math.Round(totalScore / questionList.Count);

        var submission = new Submission
        {
            UserId = request.UserId,
            ExerciseId = request.ExerciseId,
            SkillType = 3,
            TotalScore = averageScore,
            TotalQuestions = questionList.Count,
            CorrectCount = 0,
            TimeSpentSeconds = request.TimeSpentSeconds,
            ResultJson = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise != null)
        {
            exercise.AttemptCount++;
            await _unitOfWork.Exercises.UpdateAsync(exercise);
        }

        await _unitOfWork.SaveChangesAsync();

        return new SubmissionResultDto
        {
            SubmissionId = submission.Id,
            UserId = submission.UserId,
            ExerciseId = submission.ExerciseId,
            TotalScore = averageScore,
            CorrectCount = 0,
            TotalQuestions = questionList.Count,
            TimeSpentSeconds = request.TimeSpentSeconds,
            SubmittedAt = submission.SubmittedAt,
            Details = details
        };
    }
}