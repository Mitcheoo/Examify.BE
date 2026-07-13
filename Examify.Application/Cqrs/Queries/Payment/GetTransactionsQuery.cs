// Examify.Application/Cqrs/Queries/Payment/GetTransactionsQuery.cs
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Payment;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Payment;

public class GetTransactionsQuery : IRequest<PagedResult<TransactionDto>>
{
    public Guid UserId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Type { get; set; }

    public GetTransactionsQuery(Guid userId, int page, int pageSize, string? type = null)
    {
        UserId = userId;
        Page = page;
        PageSize = pageSize;
        Type = type;
    }
}