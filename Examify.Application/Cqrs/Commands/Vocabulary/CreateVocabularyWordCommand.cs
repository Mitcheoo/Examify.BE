// Examify.Application/Cqrs/Commands/Vocabulary/CreateVocabularyWordCommand.cs
using Examify.Application.DTOs.Vocabulary;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Vocabulary;

public class CreateVocabularyWordCommand : IRequest<VocabularyWordDto>
{
    public CreateVocabularyWordDto Dto { get; set; }

    public CreateVocabularyWordCommand(CreateVocabularyWordDto dto)
    {
        Dto = dto;
    }
}