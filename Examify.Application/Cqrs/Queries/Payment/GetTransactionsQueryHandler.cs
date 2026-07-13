// Examify.Application/Cqrs/Queries/Payment/GetTransactionsQueryHandler.cs
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Payment;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Payment;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTransactionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _unitOfWork.Transactions
            .FindAsync(t => t.UserId == request.UserId && !t.IsDeleted);

        if (!string.IsNullOrEmpty(request.Type))
        {
            transactions = transactions.Where(t => t.Type == request.Type).ToList();
        }

        var totalCount = transactions.Count();
        var items = transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter,
                Type = t.Type,
                Status = t.Status,
                Description = t.Description,
                PaymentMethod = t.PaymentMethod,
                CreatedAt = t.CreatedAt
            })
            .ToList();

        return new PagedResult<TransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}