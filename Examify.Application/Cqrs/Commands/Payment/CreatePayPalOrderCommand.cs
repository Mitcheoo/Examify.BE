// Examify.Application/Cqrs/Commands/Payment/CreatePayPalOrderCommand.cs
using Examify.Application.DTOs.Payment;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Payment;

public class CreatePayPalOrderCommand : IRequest<CreatePayPalOrderResponse>
{
    public Guid UserId { get; set; }
    public decimal AmountVND { get; set; }
    public Guid? ExerciseId { get; set; }

    public CreatePayPalOrderCommand(Guid userId, CreatePayPalOrderRequest request)
    {
        UserId = userId;
        AmountVND = request.AmountVND;
        ExerciseId = request.ExerciseId;
    }
}