// Examify.Application/Cqrs/Queries/Exercises/GetExerciseDetailQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetExerciseDetailQueryHandler : IRequestHandler<GetExerciseDetailQuery, ExerciseDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExerciseDetailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExerciseDetailDto> Handle(GetExerciseDetailQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);

        if (exercise == null)
            throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found");

        return _mapper.Map<ExerciseDetailDto>(exercise);
    }
}