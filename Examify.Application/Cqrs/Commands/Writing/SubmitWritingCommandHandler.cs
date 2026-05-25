// Examify.Application/Cqrs/Commands/Writing/SubmitWritingCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Submissions;
using System.Text.Json;

namespace Examify.Application.Cqrs.Commands.Writing;

public class SubmitWritingCommandHandler : IRequestHandler<SubmitWritingCommand, SubmissionResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIGradingService _aiGradingService;

    public SubmitWritingCommandHandler(IUnitOfWork unitOfWork, IAIGradingService aiGradingService)
    {
        _unitOfWork = unitOfWork;
        _aiGradingService = aiGradingService;
    }

    public async Task<SubmissionResultDto> Handle(SubmitWritingCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy đề bài Writing
        var questions = await _unitOfWork.WritingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId);

        var question = questions.FirstOrDefault();
        if (question == null)
            throw new Exception("Writing question not found");

        // 2. Gọi AI chấm điểm
        var aiResult = await _aiGradingService.GradeWritingAsync(request.EssayText, question.PromptText);

        // 3. Tạo submission
        var submission = new Submission
        {
            UserId = request.UserId,
            ExerciseId = request.ExerciseId,
            SkillType = 2, // Writing
            TotalScore = (short)aiResult.TotalScore,
            TotalQuestions = 1,
            CorrectCount = 0,
            TimeSpentSeconds = request.TimeSpentSeconds,
            EssayText = request.EssayText,
            ResultJson = JsonSerializer.Serialize(aiResult),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true
        };

        await _unitOfWork.Submissions.AddAsync(submission);

        // 4. Cập nhật số lượt làm bài
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
            TotalScore = submission.TotalScore,
            CorrectCount = 0,
            TotalQuestions = 1,
            TimeSpentSeconds = request.TimeSpentSeconds,
            SubmittedAt = submission.SubmittedAt
        };
    }
}