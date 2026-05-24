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
            .FindAsync(e => e.Skill == 0 && !e.IsDeleted); // 0 = Reading

        return _mapper.Map<List<ExerciseDto>>(exercises);
    }
}