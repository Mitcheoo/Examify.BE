/*// Examify.Application/Cqrs/Queries/Submissions/GetSubmissionHistoryQueryHandler.cs
using AutoMapper;
using Examify.Application.DTOs;
using Examify.Application.DTOs.Submissions;
using Examify.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Examify.Application.Cqrs.Queries.Submissions;

public class GetSubmissionHistoryQueryHandler
    : IRequestHandler<GetSubmissionHistoryQuery, PagedResult<SubmissionHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSubmissionHistoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<SubmissionHistoryDto>> Handle(
        GetSubmissionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId && s.IsGraded);

        var query = submissions.AsQueryable();

        if (request.Skill.HasValue)
        {
            query = query.Where(s => s.SkillType == request.Skill.Value);
        }

        query = query.OrderByDescending(s => s.SubmittedAt);

        var totalCount = query.Count();
        var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var result = new List<SubmissionHistoryDto>();

        foreach (var item in items)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(item.ExerciseId);
            result.Add(new SubmissionHistoryDto
            {
                Id = item.Id,
                ExerciseId = item.ExerciseId,
                ExerciseTitle = exercise?.Title ?? "Unknown",
                Skill = item.SkillType,
                SkillName = GetSkillName(item.SkillType),
                TotalScore = item.TotalScore,
                TotalQuestions = item.TotalQuestions,
                CorrectCount = item.CorrectCount,
                SubmittedAt = item.SubmittedAt
            });
        }

        return new PagedResult<SubmissionHistoryDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    private string GetSkillName(int skill)
    {
        return skill switch
        {
            0 => "Reading",
            1 => "Listening",
            2 => "Writing",
            3 => "Speaking",
            _ => "Unknown"
        };
    }
}*/