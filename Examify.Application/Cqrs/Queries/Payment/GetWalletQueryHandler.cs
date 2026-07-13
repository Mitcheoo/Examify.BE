// Examify.Application/Cqrs/Queries/Payment/GetWalletQueryHandler.cs
using Examify.Application.DTOs.Payment;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Payment;

public class GetWalletQueryHandler : IRequestHandler<GetWalletQuery, WalletDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWalletQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WalletDto> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var wallet = (await _unitOfWork.Wallets
            .FindAsync(w => w.UserId == request.UserId && !w.IsDeleted))
            .FirstOrDefault();

        if (wallet is null)
        {
            // Tạo ví mới nếu chưa có
            wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Balance = 0,
                TotalDeposited = 0,
                TotalSpent = 0,
                IsActive = true
            };
            await _unitOfWork.Wallets.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
        }

        return new WalletDto
        {
            Id = wallet.Id,
            Balance = wallet.Balance,
            TotalDeposited = wallet.TotalDeposited,
            TotalSpent = wallet.TotalSpent,
            IsActive = wallet.IsActive
        };
    }
}