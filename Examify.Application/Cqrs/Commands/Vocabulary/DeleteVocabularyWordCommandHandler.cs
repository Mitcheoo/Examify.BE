// Examify.Application/Cqrs/Commands/Vocabulary/DeleteVocabularyWordCommandHandler.cs
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Vocabulary;

public class DeleteVocabularyWordCommandHandler : IRequestHandler<DeleteVocabularyWordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVocabularyWordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteVocabularyWordCommand request, CancellationToken cancellationToken)
    {
        var word = await _unitOfWork.VocabularyWords.GetByIdAsync(request.Id);

        if (word == null || word.IsDeleted)
        {
            throw new NotFoundException($"Không tìm thấy từ vựng với ID '{request.Id}'");
        }

        word.IsDeleted = true;
        word.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.VocabularyWords.UpdateAsync(word);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}