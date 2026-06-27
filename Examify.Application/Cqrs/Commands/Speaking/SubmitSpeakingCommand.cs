// Examify.Application/Cqrs/Commands/Speaking/SubmitSpeakingCommand.cs
using MediatR;
using Microsoft.AspNetCore.Http;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Commands.Speaking;

public class SubmitSpeakingCommand : IRequest<SubmissionDetailDto>
{
    public Guid ExerciseId { get; set; }
    public Guid UserId { get; set; }

    // ✅ THÊM SessionId
    public Guid? SessionId { get; set; }

    public int TimeSpentSeconds { get; set; }
    public List<IFormFile>? AudioFiles { get; set; }
    public Dictionary<Guid, string>? Transcripts { get; set; }
}