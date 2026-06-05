// Examify.Application/Cqrs/Queries/Exercises/GetExerciseResultSummaryQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetExerciseResultSummaryQueryHandler : IRequestHandler<GetExerciseResultSummaryQuery, ExerciseResultSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExerciseResultSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ExerciseResultSummaryDto> Handle(GetExerciseResultSummaryQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found");

        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId && s.ExerciseId == request.ExerciseId);

        var submissionList = submissions.ToList();

        if (submissionList.Count == 0)
        {
            return new ExerciseResultSummaryDto
            {
                ExerciseId = exercise.Id,
                ExerciseTitle = exercise.Title,
                TotalAttempts = 0,
                HighestScore = 0,
                AverageScore = 0,
                LastScore = 0,
                ScoreHistory = new List<ScoreHistoryDto>()
            };
        }

        var scores = submissionList.Select(s => s.TotalScore).ToList();

        // ✅ Sửa lỗi: Chuyển sang double trước khi Average
        var averageScore = (short)Math.Round(scores.Select(s => (double)s).Average());

        return new ExerciseResultSummaryDto
        {
            ExerciseId = exercise.Id,
            ExerciseTitle = exercise.Title,
            TotalAttempts = submissionList.Count,
            HighestScore = scores.Max(),
            AverageScore = averageScore,
            LastScore = submissionList[^1].TotalScore,  // ✅ Dùng index thay vì Last()
            LastAttemptDate = submissionList.Max(s => s.SubmittedAt),
            ScoreHistory = submissionList
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => new ScoreHistoryDto
                {
                    SubmittedAt = s.SubmittedAt,
                    Score = s.TotalScore,
                    TimeSpentSeconds = s.TimeSpentSeconds
                })
                .ToList()
        };
    }
}