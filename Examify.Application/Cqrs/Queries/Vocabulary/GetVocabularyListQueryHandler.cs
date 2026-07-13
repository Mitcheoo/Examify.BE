// Examify.Application/Cqrs/Queries/Vocabulary/GetVocabularyListQueryHandler.cs
using AutoMapper;
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Vocabulary;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Vocabulary;

public class GetVocabularyListQueryHandler : IRequestHandler<GetVocabularyListQuery, PagedResult<VocabularyWordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetVocabularyListQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<VocabularyWordDto>> Handle(GetVocabularyListQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        // Lấy tất cả từ vựng chưa bị xóa
        var allWords = await _unitOfWork.VocabularyWords
            .FindAsync(w => !w.IsDeleted);

        var words = allWords.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower().Trim();
            words = words.Where(w =>
                w.Word.ToLower().Contains(search) ||
                w.Meaning.ToLower().Contains(search) ||
                w.Example.ToLower().Contains(search)
            );
        }

        if (!string.IsNullOrEmpty(filter.Level))
        {
            words = words.Where(w => w.Level == filter.Level);
        }

        if (!string.IsNullOrEmpty(filter.Topic))
        {
            words = words.Where(w => w.Topic == filter.Topic);
        }

        // Get user progress if logged in
        Dictionary<Guid, VocabularyProgress>? userProgress = null;
        if (request.UserId.HasValue)
        {
            var progresses = await _unitOfWork.VocabularyProgress
                .FindAsync(p => p.UserId == request.UserId.Value && !p.IsDeleted);
            userProgress = progresses.ToDictionary(p => p.VocabularyWordId, p => p);
        }

        // Filter by status (if user logged in)
        if (request.UserId.HasValue && !string.IsNullOrEmpty(filter.Status))
        {
            var progressDict = userProgress ?? new Dictionary<Guid, VocabularyProgress>();

            switch (filter.Status.ToLower())
            {
                case "mastered":
                    words = words.Where(w =>
                        progressDict.ContainsKey(w.Id) && progressDict[w.Id].IsMastered);
                    break;
                case "learning":
                    words = words.Where(w =>
                        !progressDict.ContainsKey(w.Id) || !progressDict[w.Id].IsMastered);
                    break;
                case "need_review":
                    var today = DateTime.UtcNow.Date;
                    words = words.Where(w =>
                        progressDict.ContainsKey(w.Id) &&
                        !progressDict[w.Id].IsMastered &&
                        progressDict[w.Id].NextReviewAt.HasValue &&
                        progressDict[w.Id].NextReviewAt.Value.Date <= today);
                    break;
            }
        }

        // Sort
        words = filter.SortBy?.ToLower() switch
        {
            "word" => filter.SortDescending ? words.OrderByDescending(w => w.Word) : words.OrderBy(w => w.Word),
            "level" => filter.SortDescending ? words.OrderByDescending(w => w.Level) : words.OrderBy(w => w.Level),
            "created_at" => filter.SortDescending ? words.OrderByDescending(w => w.CreatedAt) : words.OrderBy(w => w.CreatedAt),
            "review_count" => filter.SortDescending
                ? words.OrderByDescending(w => userProgress != null && userProgress.ContainsKey(w.Id) ? userProgress[w.Id].ReviewCount : 0)
                : words.OrderBy(w => userProgress != null && userProgress.ContainsKey(w.Id) ? userProgress[w.Id].ReviewCount : 0),
            _ => words.OrderBy(w => w.Word)
        };

        // Pagination
        var totalCount = words.Count();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var paginatedWords = words
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Map to DTOs with progress info
        var result = paginatedWords.Select(word =>
        {
            var dto = _mapper.Map<VocabularyWordDto>(word);

            if (request.UserId.HasValue && userProgress != null && userProgress.ContainsKey(word.Id))
            {
                var progress = userProgress[word.Id];
                dto.IsMastered = progress.IsMastered;
                dto.ReviewCount = progress.ReviewCount;
                dto.LastReviewedAt = progress.LastReviewedAt;
                dto.NextReviewAt = progress.NextReviewAt;
            }

            return dto;
        }).ToList();

        return new PagedResult<VocabularyWordDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}