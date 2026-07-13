// Examify.Application/Cqrs/Queries/Vocabulary/GetVocabularyListQuery.cs
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Vocabulary;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Vocabulary;

public class GetVocabularyListQuery : IRequest<PagedResult<VocabularyWordDto>>
{
    public VocabularyFilterDto Filter { get; set; }
    public Guid? UserId { get; set; } // Optional: để lấy progress của user

    public GetVocabularyListQuery(VocabularyFilterDto filter, Guid? userId = null)
    {
        Filter = filter;
        UserId = userId;
    }
}