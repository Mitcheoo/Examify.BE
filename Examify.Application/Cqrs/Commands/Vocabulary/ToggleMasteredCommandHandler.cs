// Examify.Application/Cqrs/Commands/Vocabulary/ToggleMasteredCommandHandler.cs
using AutoMapper;
using Examify.Application.DTOs.Vocabulary;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Vocabulary;

public class ToggleMasteredCommandHandler : IRequestHandler<ToggleMasteredCommand, VocabularyProgressDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ToggleMasteredCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VocabularyProgressDto> Handle(ToggleMasteredCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra từ vựng tồn tại
        var word = await _unitOfWork.VocabularyWords.GetByIdAsync(request.WordId);
        if (word == null || word.IsDeleted)
        {
            throw new NotFoundException($"Không tìm thấy từ vựng với ID '{request.WordId}'");
        }

        var progresses = await _unitOfWork.VocabularyProgress
            .FindAsync(p => p.UserId == request.UserId && p.VocabularyWordId == request.WordId && !p.IsDeleted);

        var progress = progresses.FirstOrDefault();

        if (progress == null)
        {
            // Tạo mới progress
            progress = new VocabularyProgress
            {
                UserId = request.UserId,
                VocabularyWordId = request.WordId,
                IsMastered = request.IsMastered,
                ReviewCount = 1,
                LastReviewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            if (request.IsMastered)
            {
                progress.NextReviewAt = DateTime.UtcNow.AddDays(7);
            }
            else
            {
                progress.NextReviewAt = DateTime.UtcNow.AddDays(1);
            }

            await _unitOfWork.VocabularyProgress.AddAsync(progress);
        }
        else
        {
            progress.IsMastered = request.IsMastered;
            progress.ReviewCount++;
            progress.LastReviewedAt = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            if (request.IsMastered)
            {
                int daysToAdd = Math.Min(progress.ReviewCount, 30);
                progress.NextReviewAt = DateTime.UtcNow.AddDays(daysToAdd);
                progress.StreakCount++;
            }
            else
            {
                progress.NextReviewAt = DateTime.UtcNow.AddDays(1);
                progress.StreakCount = 0;
            }

            await _unitOfWork.VocabularyProgress.UpdateAsync(progress);
        }

        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<VocabularyProgressDto>(progress);
        result.Word = word.Word;
        result.Meaning = word.Meaning;
        result.Pronunciation = word.Pronunciation;
        result.MasteryPercentage = CalculateMasteryPercentage(progress.ReviewCount, progress.CorrectCount, progress.IncorrectCount);

        return result;
    }

    private double CalculateMasteryPercentage(int reviewCount, int correctCount, int incorrectCount)
    {
        if (reviewCount == 0) return 0;
        var accuracy = (double)correctCount / (correctCount + incorrectCount + 1);
        var reviewBonus = Math.Min(reviewCount * 2, 20);
        var mastery = (accuracy * 0.8 * 100) + reviewBonus;
        return Math.Min(Math.Round(mastery, 1), 100);
    }
}