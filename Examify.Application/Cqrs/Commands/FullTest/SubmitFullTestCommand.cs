// Examify.Application/Cqrs/Commands/FullTest/SubmitFullTestCommand.cs
using MediatR;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.FullTest;

public record SubmitFullTestCommand(Guid SessionId) : IRequest<FullTestResultResponse>;