// Examify.Application/DTOs/Vocabulary/VocabularyStatsDto.cs
namespace Examify.Application.DTOs.Vocabulary;

public class VocabularyStatsDto
{
    public int TotalWords { get; set; }
    public int MasteredWords { get; set; }
    public int LearningWords { get; set; }
    public int WordsNeedReview { get; set; } // Từ cần ôn tập (next_review_at <= today)
    public double MasteryRate { get; set; } // % thành thạo

    // Weekly stats
    public List<DailyStatsDto> WeeklyProgress { get; set; } = new();
    public List<DailyStatsDto> MonthlyProgress { get; set; } = new();

    // Topics stats
    public List<TopicStatsDto> TopicStats { get; set; } = new();
}

public class DailyStatsDto
{
    public DateTime Date { get; set; }
    public int WordsLearned { get; set; }
    public int WordsMastered { get; set; }
    public int Reviews { get; set; }
}

public class TopicStatsDto
{
    public string Topic { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Mastered { get; set; }
    public int Learning { get; set; }
}