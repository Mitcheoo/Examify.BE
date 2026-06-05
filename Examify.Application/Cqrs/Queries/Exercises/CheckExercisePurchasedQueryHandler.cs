// Examify.Application/Cqrs/Queries/Exercises/CheckExercisePurchasedQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class CheckExercisePurchasedQueryHandler : IRequestHandler<CheckExercisePurchasedQuery, PurchasedStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CheckExercisePurchasedQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchasedStatusDto> Handle(CheckExercisePurchasedQuery request, CancellationToken cancellationToken)
    {
        // Tìm giao dịch mua bài thi này
        var transactions = await _unitOfWork.Transactions
            .FindAsync(t => t.UserId == request.UserId
                         && t.Type == "Purchase"
                         && t.Status == "Completed");

        // Tìm transaction có SubmissionId tương ứng với bài thi
        var purchaseTransaction = transactions.FirstOrDefault(t => t.SubmissionId.HasValue);

        if (purchaseTransaction == null)
        {
            return new PurchasedStatusDto
            {
                IsPurchased = false,
                PurchasedAt = null,
                TransactionId = null
            };
        }

        // Kiểm tra submission có thuộc exercise này không
        var submission = await _unitOfWork.Submissions.GetByIdAsync(purchaseTransaction.SubmissionId.Value);

        if (submission == null || submission.ExerciseId != request.ExerciseId)
        {
            return new PurchasedStatusDto
            {
                IsPurchased = false,
                PurchasedAt = null,
                TransactionId = null
            };
        }

        return new PurchasedStatusDto
        {
            IsPurchased = true,
            PurchasedAt = purchaseTransaction.CreatedAt,
            TransactionId = purchaseTransaction.Id
        };
    }
}