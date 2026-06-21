// Examify.Application/Cqrs/Commands/Writing/SubmitWritingCommand.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Commands.Writing;

public class SubmitWritingCommand : IRequest<SubmissionDetailDto>
{
    public Guid ExerciseId { get; set; }
    public Guid UserId { get; set; }
    public Guid? SessionId { get; set; }
    public Dictionary<Guid, string> Answers { get; set; } = new();
    public string? Task1Essay { get; set; }
    public string? Task2Essay { get; set; }
    public int TimeSpentSeconds { get; set; }
}