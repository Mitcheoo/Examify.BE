// Examify.Application/Cqrs/Commands/Auth/RegisterCommand.cs
using MediatR;
using Examify.Application.DTOs.Auth;

namespace Examify.Application.Cqrs.Commands.Auth;

public record RegisterCommand(string FullName, string UserName, string Email, string Password)
    : IRequest<bool>;