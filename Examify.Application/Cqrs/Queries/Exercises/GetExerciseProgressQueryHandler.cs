// Examify.Application/Cqrs/Queries/Exercises/GetExerciseProgressQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetExerciseProgressQueryHandler : IRequestHandler<GetExerciseProgressQuery, ExerciseProgressDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExerciseProgressQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ExerciseProgressDto> Handle(GetExerciseProgressQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found");

        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId && s.ExerciseId == request.ExerciseId);

        var submissionList = submissions.OrderBy(s => s.SubmittedAt).ToList();

        if (submissionList.Count == 0)
        {
            return new ExerciseProgressDto
            {
                ExerciseId = exercise.Id,
                ExerciseTitle = exercise.Title,
                TotalAttempts = 0,
                BestScore = 0,
                WorstScore = 0,
                AverageScore = 0,
                Improvement = 0,
                TotalTimeSpent = 0,
                WeeklyProgress = new List<WeeklyProgressDto>()
            };
        }

        var scores = submissionList.Select(s => s.TotalScore).ToList();

        // ✅ Tính trung bình đúng cách
        var averageScore = (short)Math.Round(scores.Select(s => (double)s).Average());

        // ✅ Tính tiến bộ (điểm lần cuối - điểm lần đầu)
        var improvement = submissionList.Count > 1
            ? submissionList[^1].TotalScore - submissionList[0].TotalScore
            : 0;

        var weeklyProgress = submissionList
            .GroupBy(s => new { s.SubmittedAt.Year, Week = GetWeekOfYear(s.SubmittedAt) })
            .Select(g => new WeeklyProgressDto
            {
                Year = g.Key.Year,
                Week = g.Key.Week,
                AverageScore = (short)Math.Round(g.Select(x => (double)x.TotalScore).Average()),
                Attempts = g.Count()
            })
            .OrderBy(w => w.Year)
            .ThenBy(w => w.Week)
            .ToList();

        return new ExerciseProgressDto
        {
            ExerciseId = exercise.Id,
            ExerciseTitle = exercise.Title,
            TotalAttempts = submissionList.Count,
            BestScore = scores.Max(),
            WorstScore = scores.Min(),
            AverageScore = averageScore,
            Improvement = improvement,
            TotalTimeSpent = submissionList.Sum(s => s.TimeSpentSeconds),
            WeeklyProgress = weeklyProgress
        };
    }

    // ✅ Đánh dấu là static method
    private static int GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}