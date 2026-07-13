// Examify.Application/Cqrs/Queries/Payment/GetPurchasedExercisesQueryHandler.cs
using Examify.Application.DTOs.Payment;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Payment;

public class GetPurchasedExercisesQueryHandler : IRequestHandler<GetPurchasedExercisesQuery, List<PurchasedExerciseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPurchasedExercisesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PurchasedExerciseDto>> Handle(GetPurchasedExercisesQuery request, CancellationToken cancellationToken)
    {
        var purchasedList = await _unitOfWork.PurchasedExercises
            .FindAsync(p => p.UserId == request.UserId && !p.IsDeleted);

        var result = new List<PurchasedExerciseDto>();

        foreach (var item in purchasedList)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(item.ExerciseId);
            if (exercise is not null && !exercise.IsDeleted)
            {
                result.Add(new PurchasedExerciseDto
                {
                    Id = item.Id,
                    ExerciseId = exercise.Id,
                    Title = exercise.Title,
                    Skill = exercise.Skill,
                    IsFullTest = exercise.IsFullTest,
                    PaidAmount = item.PaidAmount,
                    PurchasedAt = item.PurchasedAt
                });
            }
        }

        return result.OrderByDescending(x => x.PurchasedAt).ToList();
    }
}