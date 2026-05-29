// Examify.Application/Cqrs/Commands/Speaking/UploadAudioCommand.cs
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Examify.Application.Cqrs.Commands.Speaking;

public class UploadAudioCommand : IRequest<string>
{
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public IFormFile AudioFile { get; set; } = null!;
}