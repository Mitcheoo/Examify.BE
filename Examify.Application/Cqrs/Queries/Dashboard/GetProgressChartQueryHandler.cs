// Examify.Application/Cqrs/Queries/Dashboard/GetProgressChartQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Dashboard;

namespace Examify.Application.Cqrs.Queries.Dashboard;

public class GetProgressChartQueryHandler : IRequestHandler<GetProgressChartQuery, ProgressChartDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProgressChartQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProgressChartDto> Handle(GetProgressChartQuery request, CancellationToken cancellationToken)
    {
        var result = new ProgressChartDto
        {
            Labels = new List<string>(),
            Datasets = new List<ChartDataset>()
        };

        try
        {
            var submissions = await _unitOfWork.Submissions
                .FindAsync(s => s.UserId == request.UserId && s.IsGraded);

            var submissionsList = submissions?.ToList() ?? new List<Examify.Core.Entities.Submission>();

            var labels = new List<string>();
            var weeklyScores = new List<double>();

            for (int i = request.Weeks - 1; i >= 0; i--)
            {
                var weekStart = DateTime.UtcNow.AddDays(-(i * 7 + 7)).Date;
                var weekEnd = DateTime.UtcNow.AddDays(-(i * 7)).Date;
                labels.Add($"{weekStart:dd/MM} - {weekEnd:dd/MM}");

                var weekSubmissions = submissionsList
                    .Where(s => s.SubmittedAt.Date >= weekStart && s.SubmittedAt.Date < weekEnd)
                    .ToList();

                var avgScore = weekSubmissions.Any() ? weekSubmissions.Average(s => s.TotalScore) : 0;
                weeklyScores.Add(Math.Round(avgScore, 1));
            }

            result.Labels = labels;
            result.Datasets.Add(new ChartDataset
            {
                Label = "Điểm trung bình",
                Data = weeklyScores,
                BorderColor = "#667eea",
                BackgroundColor = "rgba(102,126,234,0.1)",
                Fill = true,
                BorderWidth = 2
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetProgressChartQueryHandler: {ex.Message}");
        }

        return result;
    }
}