// Examify.Application/Cqrs/Commands/Vocabulary/DeleteVocabularyWordCommand.cs
using MediatR;

namespace Examify.Application.Cqrs.Commands.Vocabulary;

public class DeleteVocabularyWordCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteVocabularyWordCommand(Guid id)
    {
        Id = id;
    }
}