// Examify.Application/Cqrs/Commands/Users/UpdateUserCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;

namespace Examify.Application.Cqrs.Commands.Users;

public record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : IRequest<UserDto>;