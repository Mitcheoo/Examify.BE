// Examify.Application/Cqrs/Queries/Vocabulary/GetVocabularyDetailQueryHandler.cs
using AutoMapper;
using Examify.Application.DTOs.Vocabulary;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Vocabulary;

public class GetVocabularyDetailQueryHandler : IRequestHandler<GetVocabularyDetailQuery, VocabularyWordDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetVocabularyDetailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VocabularyWordDto> Handle(GetVocabularyDetailQuery request, CancellationToken cancellationToken)
    {
        var word = await _unitOfWork.VocabularyWords.GetByIdAsync(request.Id);

        if (word == null || word.IsDeleted)
        {
            throw new NotFoundException($"Không tìm thấy từ vựng với ID '{request.Id}'");
        }

        var dto = _mapper.Map<VocabularyWordDto>(word);

        // Get user progress if logged in
        if (request.UserId.HasValue)
        {
            var progresses = await _unitOfWork.VocabularyProgress
                .FindAsync(p => p.UserId == request.UserId.Value &&
                               p.VocabularyWordId == request.Id &&
                               !p.IsDeleted);

            var progress = progresses.FirstOrDefault();
            if (progress != null)
            {
                dto.IsMastered = progress.IsMastered;
                dto.ReviewCount = progress.ReviewCount;
                dto.LastReviewedAt = progress.LastReviewedAt;
                dto.NextReviewAt = progress.NextReviewAt;
            }
        }

        return dto;
    }
}