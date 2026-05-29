// Examify.Application/Cqrs/Commands/Reading/SubmitReadingCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Commands.Reading;

public class SubmitReadingCommandHandler : IRequestHandler<SubmitReadingCommand, SubmissionResultDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitReadingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmissionResultDto> Handle(SubmitReadingCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy câu hỏi
        var questions = await _unitOfWork.ReadingQuestions
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
            SkillType = 0,
            TotalScore = totalScore,
            TotalQuestions = questionList.Count,
            CorrectCount = correctCount,
            TimeSpentSeconds = request.TimeSpentSeconds,
            SubmittedAt = DateTime.UtcNow
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        await _unitOfWork.SaveChangesAsync();

        // 5. Trả về kết quả
        return new SubmissionResultDto
        {
            SubmissionId = submission.Id,
            TotalScore = totalScore,
            CorrectCount = correctCount,
            TotalQuestions = questionList.Count,
            SubmittedAt = submission.SubmittedAt
        };
    }
}