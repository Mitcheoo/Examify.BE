// Examify.Application/Cqrs/Commands/Writing/SubmitWritingCommand.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Commands.Writing;

public class SubmitWritingCommand : IRequest<SubmissionResultDto>
{
    public Guid ExerciseId { get; set; }
    public Guid UserId { get; set; }
    public string EssayText { get; set; } = string.Empty;
    public int TimeSpentSeconds { get; set; }
}