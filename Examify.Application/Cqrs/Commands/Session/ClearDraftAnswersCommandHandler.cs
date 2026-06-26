// 📁 Examify.Application/Cqrs/Commands/Session/ClearDraftAnswersCommandHandler.cs

using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Examify.Application.Cqrs.Commands.Session;

public sealed class ClearDraftAnswersCommandHandler : IRequestHandler<ClearDraftAnswersCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ClearDraftAnswersCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ClearDraftAnswersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // ✅ TÌM TẤT CẢ SESSION ANSWERS CHƯA BỊ XÓA
            var answers = await _unitOfWork.SessionAnswers
                .FindAsync(a => a.SessionId == request.SessionId && !a.IsDeleted);

            if (answers == null || !answers.Any())
            {
                Console.WriteLine($"⚠️ No draft answers found for session {request.SessionId}");
                return true; // ✅ TRẢ VỀ TRUE THAY VÌ NÉM LỖI
            }

            int deletedCount = 0;
            foreach (var answer in answers)
            {
                answer.IsDeleted = true;
                await _unitOfWork.SessionAnswers.UpdateAsync(answer);
                deletedCount++;
            }

            await _unitOfWork.SaveChangesAsync();
            Console.WriteLine($"✅ Deleted {deletedCount} draft answers for session {request.SessionId}");
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // ✅ BẮT LỖI CONCURRENCY VÀ BỎ QUA
            Console.WriteLine($"⚠️ Concurrency error while deleting draft answers: {ex.Message}");
            return true; // ✅ KHÔNG NÉM LỖI, VẪN TRẢ VỀ TRUE
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting draft answers: {ex.Message}");
            return false;
        }
    }
}