// Examify.Application/Cqrs/Queries/Dashboard/GetDashboardStatsQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace Examify.Application.Cqrs.Queries.Dashboard;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId && s.IsGraded);

        var submissionsList = submissions.ToList();

        // Tính điểm theo kỹ năng
        var readingSubs = submissionsList.Where(s => s.SkillType == 0);
        var listeningSubs = submissionsList.Where(s => s.SkillType == 1);
        var writingSubs = submissionsList.Where(s => s.SkillType == 2);
        var speakingSubs = submissionsList.Where(s => s.SkillType == 3);
        var fullTestSubs = submissionsList.Where(s => s.SkillType == 4);

        var readingScore = readingSubs.Any() ? readingSubs.Average(s => s.TotalScore) : 0;
        var listeningScore = listeningSubs.Any() ? listeningSubs.Average(s => s.TotalScore) : 0;
        var writingScore = writingSubs.Any() ? writingSubs.Average(s => s.TotalScore) : 0;
        var speakingScore = speakingSubs.Any() ? speakingSubs.Average(s => s.TotalScore) : 0;
        var fullTestScore = fullTestSubs.Any() ? fullTestSubs.Average(s => s.TotalScore) : 0;

        var averageScore = submissionsList.Any() ? submissionsList.Average(s => s.TotalScore) : 0;
        var totalTimeSpent = (int)submissionsList.Sum(s => s.TimeSpentSeconds) / 60; // phút

        // Tính xếp hạng
        var allUserScores = await _unitOfWork.Submissions
            .FindAsync(s => s.SkillType == 4 && s.IsGraded);

        var userTotalScore = fullTestSubs.Any() ? fullTestSubs.Max(s => s.TotalScore) : 0;
        var rank = allUserScores
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, MaxScore = g.Max(s => s.TotalScore) })
            .OrderByDescending(x => x.MaxScore)
            .ToList();

        var userRank = rank.FindIndex(x => x.UserId == request.UserId) + 1;

        // Tính số ngày học liên tiếp
        var dates = submissionsList.Select(s => s.SubmittedAt.Date).Distinct().OrderBy(d => d).ToList();
        var streak = CalculateStreak(dates);

        return new DashboardStatsDto
        {
            TotalExercises = submissionsList.Count,
            AverageScore = Math.Round(averageScore, 1),
            TotalTimeSpent = totalTimeSpent,
            CurrentStreak = streak,
            ReadingScore = Math.Round(readingScore, 1),
            ListeningScore = Math.Round(listeningScore, 1),
            WritingScore = Math.Round(writingScore, 1),
            SpeakingScore = Math.Round(speakingScore, 1),
            FullTestScore = Math.Round(fullTestScore, 1),
            ReadingCount = readingSubs.Count(),
            ListeningCount = listeningSubs.Count(),
            WritingCount = writingSubs.Count(),
            SpeakingCount = speakingSubs.Count(),
            FullTestCount = fullTestSubs.Count(),
            LeaderboardRank = userRank,
            TotalUsers = rank.Count
        };
    }

    private int CalculateStreak(List<DateTime> dates)
    {
        if (!dates.Any()) return 0;

        var streak = 1;
        var current = dates.Last();

        for (int i = dates.Count - 2; i >= 0; i--)
        {
            if ((current - dates[i]).Days == 1)
            {
                streak++;
                current = dates[i];
            }
            else
            {
                break;
            }
        }
        return streak;
    }
}