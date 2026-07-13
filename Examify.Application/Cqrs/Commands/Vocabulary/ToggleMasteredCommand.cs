// Examify.Application/Cqrs/Commands/Vocabulary/ToggleMasteredCommand.cs
using Examify.Application.DTOs.Vocabulary;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Vocabulary;

public class ToggleMasteredCommand : IRequest<VocabularyProgressDto>
{
    public Guid WordId { get; set; }
    public Guid UserId { get; set; }
    public bool IsMastered { get; set; }

    public ToggleMasteredCommand(Guid wordId, Guid userId, bool isMastered)
    {
        WordId = wordId;
        UserId = userId;
        IsMastered = isMastered;
    }
}