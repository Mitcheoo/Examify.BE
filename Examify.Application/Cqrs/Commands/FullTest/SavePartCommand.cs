// Examify.Application/Cqrs/Commands/FullTest/SavePartCommand.cs
using MediatR;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.FullTest;

public record SavePartCommand(
    Guid SessionId,
    int PartNumber,
    Dictionary<Guid, string> Answers,
    string? EssayText,
    string? AudioUrl,
    int TimeSpentSeconds
) : IRequest<bool>;