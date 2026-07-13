// Examify.Application/Cqrs/Commands/Payment/PurchaseExerciseCommand.cs
using Examify.Application.DTOs.Payment;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Payment;

public class PurchaseExerciseCommand : IRequest<PurchaseResultDto>
{
    public Guid UserId { get; set; }
    public Guid ExerciseId { get; set; }

    public PurchaseExerciseCommand(Guid userId, PurchaseRequest request)
    {
        UserId = userId;
        ExerciseId = request.ExerciseId;
    }
}