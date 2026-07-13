/*// Examify.Application/Cqrs/Queries/Submissions/GetSubmissionHistoryQuery.cs
using Examify.Application.DTOs;
using Examify.Application.DTOs.Submissions;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Submissions;

public class GetSubmissionHistoryQuery : IRequest<PagedResult<SubmissionHistoryDto>>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? Skill { get; set; }
}*/