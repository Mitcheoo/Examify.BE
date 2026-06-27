// Examify.Application/Cqrs/Commands/Users/CreateUserCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;

namespace Examify.Application.Cqrs.Commands.Users;

public record CreateUserCommand(CreateUserDto Dto) : IRequest<UserDto>;