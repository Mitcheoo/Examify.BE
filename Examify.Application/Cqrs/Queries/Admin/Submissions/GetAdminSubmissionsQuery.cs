// Examify.Application/Cqrs/Queries/Admin/Submissions/GetAdminSubmissionsQuery.cs
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Common;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Admin.Submissions;

public class GetAdminSubmissionsQuery : IRequest<PagedResult<AdminSubmissionDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Skill { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; } = "SubmittedAt";
    public bool SortDescending { get; set; } = true;
}