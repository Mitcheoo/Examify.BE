// Examify.Application/Cqrs/Queries/Admin/Submissions/GetAdminSubmissionStatsQueryHandler.cs
using Examify.Application.DTOs.Admin;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Admin.Submissions;

public class GetAdminSubmissionStatsQueryHandler : IRequestHandler<GetAdminSubmissionStatsQuery, AdminSubmissionStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminSubmissionStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminSubmissionStatsDto> Handle(GetAdminSubmissionStatsQuery request, CancellationToken cancellationToken)
    {
        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => !s.IsDeleted);

        var total = submissions.Count();
        var graded = submissions.Where(s => s.IsGraded).ToList();
        var passed = graded.Count(s => s.TotalScore >= 4);
        var failed = graded.Count(s => s.TotalScore < 4);
        var inProgress = total - graded.Count;
        var avgScore = graded.Any() ? Math.Round(graded.Average(s => s.TotalScore), 1) : 0;

        // 📅 Daily stats for last X days
        var dailyStats = new List<DailySubmissionStatsDto>();
        var startDate = DateTime.UtcNow.AddDays(-request.Days + 1);

        for (var date = startDate.Date; date <= DateTime.UtcNow.Date; date = date.AddDays(1))
        {
            var dayStart = date;
            var dayEnd = date.AddDays(1);

            var daySubmissions = submissions
                .Where(s => s.SubmittedAt >= dayStart && s.SubmittedAt < dayEnd)
                .ToList();

            var dayGraded = daySubmissions.Where(s => s.IsGraded).ToList();

            dailyStats.Add(new DailySubmissionStatsDto
            {
                Date = date,
                Count = daySubmissions.Count,
                AverageScore = dayGraded.Any() ? Math.Round(dayGraded.Average(s => s.TotalScore), 1) : 0
            });
        }

        return new AdminSubmissionStatsDto
        {
            TotalSubmissions = total,
            PassedCount = passed,
            FailedCount = failed,
            InProgressCount = inProgress,
            AverageScore = avgScore,
            DailyStats = dailyStats
        };
    }
}