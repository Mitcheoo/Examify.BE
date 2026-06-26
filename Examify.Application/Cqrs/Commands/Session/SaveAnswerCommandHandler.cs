// 📁 Examify.Application/Cqrs/Commands/Session/SaveAnswerCommandHandler.cs

using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Examify.Application.Cqrs.Commands.Session;

public sealed class SaveAnswerCommandHandler : IRequestHandler<SaveAnswerCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public SaveAnswerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SaveAnswerCommand request, CancellationToken cancellationToken)
    {
        // ✅ SỬ DỤNG SEMAPHORE ĐỂ TRÁNH RACE CONDITION
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // 1. Kiểm tra session tồn tại
            var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId);
            if (session is null)
                throw new NotFoundException("Session not found");
            // ✅ BỎ QUA NẾU SESSION ĐÃ COMPLETED (KHÔNG NÉM LỖI)
            if (session.Status == 1)
            {
                Console.WriteLine($"⚠️ Session {session.Id} already completed, skipping save");
                return true;
            }

            // 2. Kiểm tra session chưa hoàn thành
            if (session.Status == 1)
                throw new BadRequestException("Session already completed, cannot save answers");

            // 3. Lấy tất cả answers hiện có (chỉ lấy những câu chưa submit)
            var existingAnswers = await _unitOfWork.SessionAnswers
                .FindAsync(a => a.SessionId == request.SessionId && !a.IsSubmitted && !a.IsDeleted);

            var existingDict = existingAnswers.ToDictionary(a => a.QuestionId);

            var answersToAdd = new List<SessionAnswer>();

            foreach (var answerDto in request.Answers)
            {
                // Bỏ qua nếu QuestionId rỗng
                if (answerDto.QuestionId == Guid.Empty)
                    continue;

                if (existingDict.TryGetValue(answerDto.QuestionId, out var existing))
                {
                    // ✅ UPDATE
                    existing.UserAnswer = answerDto.UserAnswer ?? existing.UserAnswer;
                    existing.AudioUrl = answerDto.AudioUrl ?? existing.AudioUrl;
                    existing.Transcript = answerDto.Transcript ?? existing.Transcript;
                    existing.SkillType = answerDto.SkillType;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.IsSubmitted = false;

                    await _unitOfWork.SessionAnswers.UpdateAsync(existing);
                    Console.WriteLine($"✅ Updated answer for question: {answerDto.QuestionId}");
                }
                else
                {
                    // ✅ INSERT
                    var newAnswer = new SessionAnswer
                    {
                        Id = Guid.NewGuid(),
                        SessionId = request.SessionId,
                        QuestionId = answerDto.QuestionId,
                        SkillType = answerDto.SkillType,
                        UserAnswer = answerDto.UserAnswer,
                        AudioUrl = answerDto.AudioUrl,
                        Transcript = answerDto.Transcript,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsSubmitted = false,
                        IsDeleted = false
                    };

                    answersToAdd.Add(newAnswer);
                    Console.WriteLine($"✅ Prepared insert for question: {answerDto.QuestionId}");
                }
            }

            // ✅ BULK INSERT (chỉ insert những câu chưa có)
            if (answersToAdd.Any())
            {
                // ✅ KIỂM TRA LẠI TRƯỚC KHI INSERT
                var existingIds = existingDict.Keys.ToHashSet();
                var toInsert = answersToAdd.Where(a => !existingIds.Contains(a.QuestionId)).ToList();

                foreach (var answer in toInsert)
                {
                    await _unitOfWork.SessionAnswers.AddAsync(answer);
                    Console.WriteLine($"✅ Inserted answer for question: {answer.QuestionId}");
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}