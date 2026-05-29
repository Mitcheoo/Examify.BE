// Examify.Application/Cqrs/Commands/Speaking/UploadAudioCommandHandler.cs
using MediatR;
using Examify.Core.Interfaces;

namespace Examify.Application.Cqrs.Commands.Speaking;

public class UploadAudioCommandHandler : IRequestHandler<UploadAudioCommand, string>
{
    private readonly IFileStorageService _fileStorageService;

    public UploadAudioCommandHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<string> Handle(UploadAudioCommand request, CancellationToken cancellationToken)
    {
        var audioUrl = await _fileStorageService.UploadFileAsync(request.AudioFile, "speaking-audios");
        return audioUrl;
    }
}