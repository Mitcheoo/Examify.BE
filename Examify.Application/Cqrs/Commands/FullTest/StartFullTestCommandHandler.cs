// Examify.Application/Cqrs/Commands/FullTest/StartFullTestCommandHandler.cs

using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.FullTest;

public class StartFullTestCommandHandler : IRequestHandler<StartFullTestCommand, StartFullTestResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public StartFullTestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StartFullTestResponse> Handle(StartFullTestCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId; // ✅ Lấy từ Command

        // 1. Lấy Full Test
        var fullTest = await _unitOfWork.Exercises.GetByIdAsync(request.FullTestId);
        if (fullTest is null || !fullTest.IsFullTest)
            throw new NotFoundException($"Full Test with ID {request.FullTestId} not found");

        // ============================================================
        // ✅ KIỂM TRA BÀI THI CÓ PHÍ VÀ TRỪ TIỀN
        // ============================================================

        if (!fullTest.IsFree && fullTest.Price > 0)
        {
            Console.WriteLine($"💰 Full Test có phí: {fullTest.Price} VND");

            // Kiểm tra user đã mua bài thi này chưa
            var purchased = (await _unitOfWork.PurchasedExercises
                .FindAsync(p => p.UserId == userId && p.ExerciseId == request.FullTestId && !p.IsDeleted))
                .FirstOrDefault();

            if (purchased == null)
            {
                Console.WriteLine($"🔍 User chưa mua bài thi này. Kiểm tra số dư...");

                var wallet = (await _unitOfWork.Wallets
                    .FindAsync(w => w.UserId == userId && !w.IsDeleted))
                    .FirstOrDefault();

                if (wallet is null)
                {
                    throw new InvalidOperationException("Bạn chưa có ví. Vui lòng nạp tiền trước!");
                }

                if (wallet.Balance < fullTest.Price)
                {
                    throw new InvalidOperationException(
                        $"Số dư không đủ. Cần {fullTest.Price:N0} VND, hiện có {wallet.Balance:N0} VND."
                    );
                }

                // Trừ tiền
                var balanceBefore = wallet.Balance;
                wallet.Balance -= fullTest.Price;
                wallet.TotalSpent += fullTest.Price;

                // Tạo PurchasedExercise
                var purchasedExercise = new PurchasedExercise
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ExerciseId = request.FullTestId,
                    PaidAmount = fullTest.Price,
                    PurchasedAt = DateTime.UtcNow
                };

                // Tạo Transaction
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    WalletId = wallet.Id,
                    ExerciseId = request.FullTestId,
                    Amount = -fullTest.Price,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,
                    Type = "Purchase",
                    Status = "Success",
                    Description = $"Mua Full Test: {fullTest.Title}",
                    PaymentMethod = "Wallet"
                };

                await _unitOfWork.Wallets.UpdateAsync(wallet);
                await _unitOfWork.PurchasedExercises.AddAsync(purchasedExercise);
                await _unitOfWork.Transactions.AddAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                Console.WriteLine($"✅ Đã trừ {fullTest.Price:N0} VND. Số dư mới: {wallet.Balance:N0} VND");
            }
            else
            {
                Console.WriteLine($"✅ User đã mua bài thi này rồi.");
            }
        }

        // ... Phần còn lại giữ nguyên (tạo session, exercise con) ...

        // 2. Xóa session cũ
        var oldSessions = await _unitOfWork.FullTestSessions
            .FindAsync(s => s.UserId == userId && s.FullTestId == request.FullTestId && s.Status == 0);

        var oldSession = oldSessions.FirstOrDefault();
        if (oldSession != null)
        {
            var oldAnswers = await _unitOfWork.SessionAnswers
                .FindAsync(a => a.SessionId == oldSession.Id);

            foreach (var answer in oldAnswers)
            {
                await _unitOfWork.SessionAnswers.DeleteAsync(answer);
            }

            oldSession.Status = 2;
            await _unitOfWork.FullTestSessions.UpdateAsync(oldSession);
        }

        // 3. Tạo hoặc lấy exercise con
        var readingId = await GetOrCreateChildExercise(fullTest.Id, 0, "Reading");
        var listeningId = await GetOrCreateChildExercise(fullTest.Id, 1, "Listening");
        var writingId = await GetOrCreateChildExercise(fullTest.Id, 2, "Writing");
        var speakingId = await GetOrCreateChildExercise(fullTest.Id, 3, "Speaking");

        // 4. Cập nhật Full Test
        bool isUpdated = false;
        if (!fullTest.ReadingExerciseId.HasValue && readingId.HasValue)
        {
            fullTest.ReadingExerciseId = readingId.Value;
            isUpdated = true;
        }
        if (!fullTest.ListeningExerciseId.HasValue && listeningId.HasValue)
        {
            fullTest.ListeningExerciseId = listeningId.Value;
            isUpdated = true;
        }
        if (!fullTest.WritingExerciseId.HasValue && writingId.HasValue)
        {
            fullTest.WritingExerciseId = writingId.Value;
            isUpdated = true;
        }
        if (!fullTest.SpeakingExerciseId.HasValue && speakingId.HasValue)
        {
            fullTest.SpeakingExerciseId = speakingId.Value;
            isUpdated = true;
        }

        if (isUpdated)
        {
            await _unitOfWork.Exercises.UpdateAsync(fullTest);
            await _unitOfWork.SaveChangesAsync();
        }

        // 5. Tạo Session mới
        var session = new FullTestSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullTestId = request.FullTestId,
            StartTime = DateTime.UtcNow,
            Status = 0,
            CurrentPart = 1,
            ReadingExerciseId = readingId,
            ListeningExerciseId = listeningId,
            WritingExerciseId = writingId,
            SpeakingExerciseId = speakingId,
            ReadingTimeSpent = 0,
            ListeningTimeSpent = 0,
            WritingTimeSpent = 0,
            SpeakingTimeSpent = 0,
            TotalScore = 0,
            AttemptCounted = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.FullTestSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        // 6. Tạo response
        var parts = new List<FullTestPartDto>();
        var exerciseIds = new[] { readingId, listeningId, writingId, speakingId };
        var partNames = new[] { "Reading", "Listening", "Writing", "Speaking" };
        var timeLimits = new[] { 3600, 2100, 3600, 1020 };

        for (int i = 0; i < 4; i++)
        {
            if (exerciseIds[i].HasValue)
            {
                var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseIds[i].Value);
                parts.Add(new FullTestPartDto
                {
                    PartNumber = i + 1,
                    PartName = partNames[i],
                    ExerciseId = exerciseIds[i].Value,
                    TimeLimitSeconds = exercise?.TimeLimitSeconds ?? timeLimits[i],
                    TotalQuestions = exercise?.TotalQuestions ?? 3,
                    IsCompleted = false
                });
            }
        }

        return new StartFullTestResponse
        {
            SessionId = session.Id,
            StartTime = session.StartTime,
            Parts = parts
        };
    }

    private async Task<Guid?> GetOrCreateChildExercise(Guid fullTestId, int skill, string skillName)
    {
        var existing = await _unitOfWork.Exercises
            .FindAsync(e => e.FullTestId == fullTestId && e.Skill == skill && !e.IsDeleted);

        if (existing.Any())
            return existing.First().Id;

        var newExercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Skill = skill,
            Title = $"VSTEP {skillName} Test - {DateTime.Now:yyyy-MM-dd HH:mm}",
            Description = $"Bài thi {skillName} cho Full Test",
            FullTestId = fullTestId,
            IsFullTest = false,
            TotalParts = 1,
            TotalQuestions = 3,
            TimeLimitSeconds = skill switch
            {
                0 => 3600,
                1 => 2100,
                2 => 3600,
                3 => 1020,
                _ => 3600
            },
            Difficulty = 3,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Exercises.AddAsync(newExercise);
        await _unitOfWork.SaveChangesAsync();

        return newExercise.Id;
    }
}