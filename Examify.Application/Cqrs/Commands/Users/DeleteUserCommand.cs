// Examify.Application/Cqrs/Commands/Users/DeleteUserCommand.cs
using MediatR;

namespace Examify.Application.Cqrs.Commands.Users;

public record DeleteUserCommand(Guid Id) : IRequest<bool>;