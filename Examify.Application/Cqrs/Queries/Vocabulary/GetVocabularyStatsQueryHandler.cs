// Examify.Application/Cqrs/Queries/Vocabulary/GetVocabularyStatsQueryHandler.cs
using Examify.Application.DTOs.Vocabulary;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Vocabulary;

public class GetVocabularyStatsQueryHandler : IRequestHandler<GetVocabularyStatsQuery, VocabularyStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVocabularyStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VocabularyStatsDto> Handle(GetVocabularyStatsQuery request, CancellationToken cancellationToken)
    {
        // Get all vocabulary words
        var allWords = await _unitOfWork.VocabularyWords.FindAsync(w => !w.IsDeleted);
        var totalWords = allWords.Count();

        // Get user progress
        var userProgresses = await _unitOfWork.VocabularyProgress
            .FindAsync(p => p.UserId == request.UserId && !p.IsDeleted);

        var masteredWords = userProgresses.Count(p => p.IsMastered);
        var learningWords = totalWords - masteredWords;
        var needReviewCount = userProgresses.Count(p =>
            !p.IsMastered &&
            p.NextReviewAt.HasValue &&
            p.NextReviewAt.Value.Date <= DateTime.UtcNow.Date);

        // Weekly progress (last 7 days)
        var weeklyProgress = new List<DailyStatsDto>();
        var today = DateTime.UtcNow.Date;
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayProgress = userProgresses
                .Where(p => p.LastReviewedAt.HasValue && p.LastReviewedAt.Value.Date == date)
                .ToList();

            weeklyProgress.Add(new DailyStatsDto
            {
                Date = date,
                WordsLearned = dayProgress.Count(p => p.ReviewCount == 1),
                WordsMastered = dayProgress.Count(p => p.IsMastered),
                Reviews = dayProgress.Count()
            });
        }

        // Monthly progress
        var monthlyProgress = new List<DailyStatsDto>();
        var monthStart = today.AddDays(-request.Days + 1);
        for (var date = monthStart; date <= today; date = date.AddDays(1))
        {
            var dayProgress = userProgresses
                .Where(p => p.LastReviewedAt.HasValue && p.LastReviewedAt.Value.Date == date)
                .ToList();

            monthlyProgress.Add(new DailyStatsDto
            {
                Date = date,
                WordsLearned = dayProgress.Count(p => p.ReviewCount == 1),
                WordsMastered = dayProgress.Count(p => p.IsMastered),
                Reviews = dayProgress.Count()
            });
        }

        // Topic stats
        var topics = allWords.Select(w => w.Topic).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();
        var topicStats = new List<TopicStatsDto>();

        foreach (var topic in topics)
        {
            var wordsInTopic = allWords.Where(w => w.Topic == topic).ToList();
            var masteredInTopic = wordsInTopic.Count(w =>
                userProgresses.Any(p => p.VocabularyWordId == w.Id && p.IsMastered));

            topicStats.Add(new TopicStatsDto
            {
                Topic = topic!,
                Total = wordsInTopic.Count,
                Mastered = masteredInTopic,
                Learning = wordsInTopic.Count - masteredInTopic
            });
        }

        return new VocabularyStatsDto
        {
            TotalWords = totalWords,
            MasteredWords = masteredWords,
            LearningWords = learningWords,
            WordsNeedReview = needReviewCount,
            MasteryRate = totalWords > 0 ? Math.Round((double)masteredWords / totalWords * 100, 1) : 0,
            WeeklyProgress = weeklyProgress,
            MonthlyProgress = monthlyProgress,
            TopicStats = topicStats
        };
    }
}