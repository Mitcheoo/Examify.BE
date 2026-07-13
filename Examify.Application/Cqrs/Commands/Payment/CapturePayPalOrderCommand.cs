// Examify.Application/Cqrs/Commands/Payment/CapturePayPalOrderCommand.cs
using Examify.Application.DTOs.Payment;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Payment;

public class CapturePayPalOrderCommand : IRequest<CapturePayPalOrderResponse>
{
    public Guid UserId { get; set; }
    public string OrderCode { get; set; }
    public string PayPalOrderId { get; set; }

    public CapturePayPalOrderCommand(Guid userId, string orderCode, CapturePayPalOrderRequest request)
    {
        UserId = userId;
        OrderCode = orderCode;
        PayPalOrderId = request.PayPalOrderId;
    }
}