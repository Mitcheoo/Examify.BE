// Examify.Application/DTOs/Dashboard/DashboardStatsDto.cs
namespace Examify.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalExercises { get; set; }      // Tổng số bài đã làm
    public double AverageScore { get; set; }     // Điểm trung bình
    public int TotalTimeSpent { get; set; }      // Tổng thời gian học (phút)
    public int CurrentStreak { get; set; }       // Số ngày học liên tiếp

    // Điểm theo từng kỹ năng
    public double ReadingScore { get; set; }
    public double ListeningScore { get; set; }
    public double WritingScore { get; set; }
    public double SpeakingScore { get; set; }
    public double FullTestScore { get; set; }

    // Số lượng bài đã làm theo từng kỹ năng
    public int ReadingCount { get; set; }
    public int ListeningCount { get; set; }
    public int WritingCount { get; set; }
    public int SpeakingCount { get; set; }
    public int FullTestCount { get; set; }

    // Xếp hạng
    public int LeaderboardRank { get; set; }
    public int TotalUsers { get; set; }
}