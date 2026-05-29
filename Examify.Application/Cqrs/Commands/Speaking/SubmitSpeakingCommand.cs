// Examify.Application/Cqrs/Commands/Speaking/SubmitSpeakingCommand.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Commands.Speaking;

public class SubmitSpeakingCommand : IRequest<SubmissionResultDto>
{
    public Guid ExerciseId { get; set; }
    public Guid UserId { get; set; }
    public Dictionary<Guid, string> Answers { get; set; } = new(); // QuestionId -> Transcript
    public Dictionary<Guid, string> AudioUrls { get; set; } = new(); // QuestionId -> AudioUrl
    public int TimeSpentSeconds { get; set; }
}