// Examify.Application/Cqrs/Queries/Exercises/GetExercisesListQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetExercisesListQueryHandler : IRequestHandler<GetExercisesListQuery, List<ExerciseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExercisesListQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ExerciseDto>> Handle(GetExercisesListQuery request, CancellationToken cancellationToken)
    {
        var exercises = await _unitOfWork.Exercises
            .FindAsync(e => !e.IsDeleted && !e.IsFullTest);

        if (request.Skill.HasValue)
        {
            exercises = exercises.Where(e => e.Skill == (int)request.Skill.Value).ToList();
        }

        exercises = exercises.OrderBy(e => e.Skill).ThenBy(e => e.Title).ToList();
        return _mapper.Map<List<ExerciseDto>>(exercises);
    }
}