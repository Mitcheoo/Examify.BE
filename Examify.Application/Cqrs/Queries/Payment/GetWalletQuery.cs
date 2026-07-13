// Examify.Application/Cqrs/Queries/Payment/GetWalletQuery.cs
using Examify.Application.DTOs.Payment;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Payment;

public class GetWalletQuery : IRequest<WalletDto>
{
    public Guid UserId { get; set; }

    public GetWalletQuery(Guid userId)
    {
        UserId = userId;
    }
}