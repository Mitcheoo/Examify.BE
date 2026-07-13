// Examify.Application/Cqrs/Queries/Admin/Submissions/GetAdminSubmissionStatsQuery.cs
using Examify.Application.DTOs.Admin;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Admin.Submissions;

public class GetAdminSubmissionStatsQuery : IRequest<AdminSubmissionStatsDto>
{
    public int Days { get; set; } = 30;
}