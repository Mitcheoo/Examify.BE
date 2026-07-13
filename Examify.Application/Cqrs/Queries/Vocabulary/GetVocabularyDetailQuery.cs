// Examify.Application/Cqrs/Queries/Vocabulary/GetVocabularyDetailQuery.cs
using Examify.Application.DTOs.Vocabulary;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Vocabulary;

public class GetVocabularyDetailQuery : IRequest<VocabularyWordDto>
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }

    public GetVocabularyDetailQuery(Guid id, Guid? userId = null)
    {
        Id = id;
        UserId = userId;
    }
}