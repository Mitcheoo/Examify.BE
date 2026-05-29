// Examify.Application/Cqrs/Commands/Auth/LoginCommand.cs
using MediatR;
using Examify.Application.DTOs.Auth;

namespace Examify.Application.Cqrs.Commands.Auth;

public record LoginCommand(string UserName, string Password) : IRequest<LoginResponseDto>;