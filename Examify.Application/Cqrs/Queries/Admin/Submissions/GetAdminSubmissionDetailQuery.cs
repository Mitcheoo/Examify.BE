// Examify.Application/Cqrs/Queries/Admin/Submissions/GetAdminSubmissionDetailQuery.cs
using Examify.Application.DTOs.Admin;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Admin.Submissions;

public class GetAdminSubmissionDetailQuery : IRequest<AdminSubmissionDto>
{
    public Guid Id { get; set; }

    public GetAdminSubmissionDetailQuery(Guid id)
    {
        Id = id;
    }
}