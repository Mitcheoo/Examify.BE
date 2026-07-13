// Examify.Application/Cqrs/Commands/Session/SubmitSessionCommand.cs
using MediatR;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.Session;

public record SubmitSessionCommand(Guid SessionId) : IRequest<FullTestResultResponse>;