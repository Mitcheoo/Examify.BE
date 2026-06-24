// Examify.Application/Cqrs/Commands/Session/SaveAnswerCommand.cs
using MediatR;
using Examify.Application.DTOs.Session;

namespace Examify.Application.Cqrs.Commands.Session;

public record SaveAnswerCommand(
    Guid SessionId,
    List<QuestionAnswerDto> Answers
) : IRequest<bool>;