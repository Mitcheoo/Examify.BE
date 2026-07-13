// Examify.Application/Cqrs/Queries/Vocabulary/GetVocabularyStatsQuery.cs
using Examify.Application.DTOs.Vocabulary;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Vocabulary;

public class GetVocabularyStatsQuery : IRequest<VocabularyStatsDto>
{
    public Guid UserId { get; set; }
    public int Days { get; set; } = 30; // Số ngày thống kê

    public GetVocabularyStatsQuery(Guid userId, int days = 30)
    {
        UserId = userId;
        Days = days;
    }
}