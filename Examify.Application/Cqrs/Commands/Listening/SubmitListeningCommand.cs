// Examify.Application/Cqrs/Commands/Listening/SubmitListeningCommand.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Commands.Listening;

public class SubmitListeningCommand : IRequest<SubmissionResultDto>
{
    public Guid ExerciseId { get; set; }
    public Guid UserId { get; set; }
    public Dictionary<Guid, string> Answers { get; set; } = new();
    public int TimeSpentSeconds { get; set; }
}