// Examify.Application/Cqrs/Commands/Session/SaveAnswerCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;

namespace Examify.Application.Cqrs.Commands.Session;

public sealed class SaveAnswerCommandHandler : IRequestHandler<SaveAnswerCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public SaveAnswerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SaveAnswerCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra session tồn tại
        var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId);
        if (session is null)
            throw new NotFoundException("Session not found");

        // 2. Kiểm tra session chưa hoàn thành
        if (session.Status == 1)
            throw new BadRequestException("Session already completed, cannot save answers");

        // 3. Lấy tất cả answers hiện có
        var existingAnswers = await _unitOfWork.SessionAnswers
            .FindAsync(a => a.SessionId == request.SessionId && !a.IsSubmitted);

        var existingDict = existingAnswers.ToDictionary(a => a.QuestionId);

        var answersToAdd = new List<SessionAnswer>();

        foreach (var answerDto in request.Answers)
        {
            // ✅ BỎ QUA NẾU QUESTIONID RỖNG
            if (answerDto.QuestionId == Guid.Empty)
                continue;

            // ✅ KHÔNG KIỂM TRA QUESTIONID CÓ TỒN TẠI TRONG DB KHÔNG
            // Chỉ cần lưu vào SessionAnswers

            if (existingDict.TryGetValue(answerDto.QuestionId, out var existing))
            {
                // UPDATE
                existing.UserAnswer = answerDto.UserAnswer ?? existing.UserAnswer;
                existing.AudioUrl = answerDto.AudioUrl ?? existing.AudioUrl;
                existing.Transcript = answerDto.Transcript ?? existing.Transcript;
                existing.SkillType = answerDto.SkillType;  // ✅ THÊM
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsSubmitted = false;

                await _unitOfWork.SessionAnswers.UpdateAsync(existing);
            }
            else
            {
                // INSERT
                var newAnswer = new SessionAnswer
                {
                    Id = Guid.NewGuid(),
                    SessionId = request.SessionId,
                    QuestionId = answerDto.QuestionId,
                    SkillType = answerDto.SkillType,
                    UserAnswer = answerDto.UserAnswer,
                    AudioUrl = answerDto.AudioUrl,
                    Transcript = answerDto.Transcript,
                    UpdatedAt = DateTime.UtcNow,
                    IsSubmitted = false,
                    IsDeleted = false  // ✅ THÊM
                };

                answersToAdd.Add(newAnswer);
            }
        }

        if (answersToAdd.Any())
        {
            foreach (var answer in answersToAdd)
            {
                await _unitOfWork.SessionAnswers.AddAsync(answer);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}