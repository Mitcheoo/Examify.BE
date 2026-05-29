// Examify.Application/Cqrs/Commands/Listening/SubmitListeningCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Commands.Listening;

public class SubmitListeningCommandHandler : IRequestHandler<SubmitListeningCommand, SubmissionResultDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitListeningCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmissionResultDto> Handle(SubmitListeningCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy câu hỏi Listening
        var questions = await _unitOfWork.ListeningQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId);

        var questionList = questions.ToList();

        // 2. Chấm điểm
        int correctCount = 0;
        foreach (var q in questionList)
        {
            if (request.Answers.TryGetValue(q.Id, out var answer) && answer == q.CorrectAnswer)
            {
                correctCount++;
            }
        }

        // 3. Tính điểm (thang 10)
        var totalScore = (short)Math.Round((double)correctCount / questionList.Count * 10);

        // 4. Tạo submission
        var submission = new Submission
        {
            UserId = request.UserId,
            ExerciseId = request.ExerciseId,
            SkillType = 1, // Listening
            TotalScore = totalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = correctCount,
            TimeSpentSeconds = request.TimeSpentSeconds,
            SubmittedAt = DateTime.UtcNow
        };

        await _unitOfWork.Submissions.AddAsync(submission);

        // 5. Cập nhật số lượt làm bài
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
            TotalScore = totalScore,
            CorrectCount = correctCount,
            TotalQuestions = questionList.Count,
            TimeSpentSeconds = request.TimeSpentSeconds,
            SubmittedAt = submission.SubmittedAt
        };
    }
}