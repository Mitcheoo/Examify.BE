// Examify.Application/Cqrs/Commands/Session/ClearDraftAnswersCommand.cs
using MediatR;

namespace Examify.Application.Cqrs.Commands.Session;

public record ClearDraftAnswersCommand(Guid SessionId) : IRequest<bool>;