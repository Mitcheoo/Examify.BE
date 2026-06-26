// Examify.Application/Cqrs/Queries/Users/GetUserByIdQuery.cs
using MediatR;
using Examify.Application.DTOs.Admin;

namespace Examify.Application.Cqrs.Queries.Users;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;