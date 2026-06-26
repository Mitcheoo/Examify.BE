// Examify.Application/Cqrs/Queries/Users/GetUsersQuery.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Common;

namespace Examify.Application.Cqrs.Queries.Users;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null
) : IRequest<PagedResult<UserDto>>;