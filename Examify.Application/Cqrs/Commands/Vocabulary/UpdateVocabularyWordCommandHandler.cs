// Examify.Application/Cqrs/Commands/Vocabulary/UpdateVocabularyWordCommandHandler.cs
using AutoMapper;
using Examify.Application.DTOs.Vocabulary;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Vocabulary;

public class UpdateVocabularyWordCommandHandler : IRequestHandler<UpdateVocabularyWordCommand, VocabularyWordDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateVocabularyWordCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VocabularyWordDto> Handle(UpdateVocabularyWordCommand request, CancellationToken cancellationToken)
    {
        var word = await _unitOfWork.VocabularyWords.GetByIdAsync(request.Id);

        if (word == null || word.IsDeleted)
        {
            throw new NotFoundException($"Không tìm thấy từ vựng với ID '{request.Id}'");
        }

        // Kiểm tra tên mới không trùng với từ khác (nếu có thay đổi Word)
        if (!string.IsNullOrEmpty(request.Dto.Word) && request.Dto.Word != word.Word)
        {
            var existingWords = await _unitOfWork.VocabularyWords
                .FindAsync(w => w.Word.ToLower() == request.Dto.Word.ToLower() &&
                               w.Id != request.Id &&
                               !w.IsDeleted);

            if (existingWords.Any())
            {
                throw new BadRequestException($"Từ vựng '{request.Dto.Word}' đã tồn tại trong hệ thống");
            }
        }

        _mapper.Map(request.Dto, word);
        word.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.VocabularyWords.UpdateAsync(word);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<VocabularyWordDto>(word);
    }
}