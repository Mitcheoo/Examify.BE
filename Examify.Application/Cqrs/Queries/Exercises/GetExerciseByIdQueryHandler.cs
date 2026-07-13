// Examify.Application/Cqrs/Queries/Exercises/GetExerciseByIdQueryHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, Exercise?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExerciseByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Exercise?> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Exercises.GetByIdAsync(request.Id);
    }
}