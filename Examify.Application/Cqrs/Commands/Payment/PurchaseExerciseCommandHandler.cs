// Examify.Application/Cqrs/Commands/Payment/PurchaseExerciseCommandHandler.cs
using Examify.Application.DTOs.Payment;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Payment;

public class PurchaseExerciseCommandHandler : IRequestHandler<PurchaseExerciseCommand, PurchaseResultDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseExerciseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseResultDto> Handle(PurchaseExerciseCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra bài thi
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise is null)
            throw new NotFoundException("Không tìm thấy bài thi");

        if (exercise.IsDeleted)
            throw new InvalidOperationException("Bài thi đã bị xóa");

        if (exercise.IsFree)
            throw new InvalidOperationException("Bài thi này miễn phí");

        // 2. Kiểm tra đã mua chưa
        var existingPurchase = (await _unitOfWork.PurchasedExercises
            .FindAsync(p => p.UserId == request.UserId && p.ExerciseId == request.ExerciseId && !p.IsDeleted))
            .FirstOrDefault();

        if (existingPurchase is not null)
            throw new InvalidOperationException("Bạn đã mua bài thi này rồi");

        // 3. Kiểm tra số dư
        var wallet = (await _unitOfWork.Wallets
            .FindAsync(w => w.UserId == request.UserId && !w.IsDeleted))
            .FirstOrDefault();

        if (wallet is null)
        {
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
        }

        if (wallet.Balance < exercise.Price)
        {
            var needMore = exercise.Price - wallet.Balance;
            throw new Exception($"Số dư không đủ. Cần thêm {needMore:N0} VND");
        }

        // 4. Trừ tiền
        var balanceBefore = wallet.Balance;
        wallet.Balance -= exercise.Price;
        wallet.TotalSpent += exercise.Price;

        // 5. Tạo PurchasedExercise
        var purchased = new PurchasedExercise
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ExerciseId = exercise.Id,
            PaidAmount = exercise.Price,
            PurchasedAt = DateTime.UtcNow
        };

        // 6. Tạo Transaction
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            WalletId = wallet.Id,
            ExerciseId = exercise.Id,
            Amount = -exercise.Price,
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.Balance,
            Type = "Purchase",
            Status = "Success",
            Description = $"Mua bài thi: {exercise.Title}",
            PaymentMethod = "Wallet"
        };

        // 7. Lưu tất cả
        await _unitOfWork.Wallets.UpdateAsync(wallet);
        await _unitOfWork.PurchasedExercises.AddAsync(purchased);
        await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // 8. Trả về kết quả
        return new PurchaseResultDto
        {
            Success = true,
            Message = $"Mua bài thi '{exercise.Title}' thành công!",
            NewBalance = wallet.Balance,
            ExerciseId = exercise.Id
        };
    }
}