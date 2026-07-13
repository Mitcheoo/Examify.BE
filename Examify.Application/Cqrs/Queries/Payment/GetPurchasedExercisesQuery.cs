// Examify.Application/Cqrs/Queries/Payment/GetPurchasedExercisesQuery.cs
using Examify.Application.DTOs.Payment;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Payment;

public class GetPurchasedExercisesQuery : IRequest<List<PurchasedExerciseDto>>
{
    public Guid UserId { get; set; }

    public GetPurchasedExercisesQuery(Guid userId)
    {
        UserId = userId;
    }
}