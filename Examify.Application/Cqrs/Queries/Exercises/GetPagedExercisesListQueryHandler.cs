// Examify.Application/Cqrs/Queries/Exercises/GetPagedExercisesListQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetPagedExercisesListQueryHandler : IRequestHandler<GetPagedExercisesListQuery, PagedResult<ExerciseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPagedExercisesListQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ExerciseDto>> Handle(GetPagedExercisesListQuery request, CancellationToken cancellationToken)
    {
        var allExercises = await _unitOfWork.Exercises
            .FindAsync(e => !e.IsDeleted);

        var exercises = allExercises.AsQueryable();

        if (request.Skill.HasValue)
        {
            exercises = exercises.Where(e => e.Skill == request.Skill.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            exercises = exercises.Where(e => e.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = exercises.Count();
        var items = exercises
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<ExerciseDto>
        {
            Items = _mapper.Map<List<ExerciseDto>>(items),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}