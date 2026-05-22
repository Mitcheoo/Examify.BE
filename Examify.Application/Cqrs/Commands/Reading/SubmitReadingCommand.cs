// Examify.Application/Cqrs/Commands/Reading/SubmitReadingCommand.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Commands.Reading;

public record SubmitReadingCommand(
    Guid ExerciseId,
    Guid UserId,
    Dictionary<Guid, string> Answers,
    int TimeSpentSeconds
) : IRequest<SubmissionResultDto>;