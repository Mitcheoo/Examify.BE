// Examify.Application/Cqrs/Commands/Vocabulary/UpdateVocabularyWordCommand.cs
using Examify.Application.DTOs.Vocabulary;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Vocabulary;

public class UpdateVocabularyWordCommand : IRequest<VocabularyWordDto>
{
    public Guid Id { get; set; }
    public UpdateVocabularyWordDto Dto { get; set; }

    public UpdateVocabularyWordCommand(Guid id, UpdateVocabularyWordDto dto)
    {
        Id = id;
        Dto = dto;
    }
}