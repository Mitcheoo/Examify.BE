// Examify.Application/Cqrs/Commands/FullTest/StartFullTestCommand.cs
using MediatR;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.FullTest;

public record StartFullTestCommand(Guid UserId) : IRequest<StartFullTestResponse>;