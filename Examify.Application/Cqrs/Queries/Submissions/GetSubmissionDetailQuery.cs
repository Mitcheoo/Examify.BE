/*// Examify.Application/Cqrs/Queries/Submissions/GetSubmissionDetailQuery.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Queries.Submissions;

public class GetSubmissionDetailQuery : IRequest<SubmissionDetailDto>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public GetSubmissionDetailQuery(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
    }
}*/