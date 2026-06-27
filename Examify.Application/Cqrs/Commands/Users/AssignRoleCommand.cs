// Examify.Application/Cqrs/Commands/Users/AssignRoleCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;

namespace Examify.Application.Cqrs.Commands.Users;

public record AssignRoleCommand(Guid UserId, AssignRoleDto Dto) : IRequest<UserDto>;