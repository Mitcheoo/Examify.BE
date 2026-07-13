// Examify.Application/Cqrs/Commands/Vocabulary/CreateVocabularyWordCommandHandler.cs
using AutoMapper;
using Examify.Application.DTOs.Vocabulary;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Vocabulary;

public class CreateVocabularyWordCommandHandler : IRequestHandler<CreateVocabularyWordCommand, VocabularyWordDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateVocabularyWordCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VocabularyWordDto> Handle(CreateVocabularyWordCommand request, CancellationToken cancellationToken)
    {
        // ❌ Không dùng GetRepository<T>()
        // var repository = _unitOfWork.GetRepository<VocabularyWord>();

        // ✅ Sử dụng trực tiếp repository từ UnitOfWork
        // Giả sử bạn đã thêm VocabularyWords vào IUnitOfWork
        var existingWords = await _unitOfWork.VocabularyWords
            .FindAsync(w => w.Word.ToLower() == request.Dto.Word.ToLower() && !w.IsDeleted);

        if (existingWords.Any())
        {
            throw new BadRequestException($"Từ vựng '{request.Dto.Word}' đã tồn tại trong hệ thống");
        }

        var word = _mapper.Map<VocabularyWord>(request.Dto);
        word.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.VocabularyWords.AddAsync(word);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<VocabularyWordDto>(word);
    }
}