// Examify.Application/Cqrs/Commands/Session/ClearDraftAnswersCommandHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;

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
        var answers = await _unitOfWork.SessionAnswers
            .FindAsync(a => a.SessionId == request.SessionId && !a.IsSubmitted);

        foreach (var answer in answers)
        {
            answer.IsDeleted = true;
            await _unitOfWork.SessionAnswers.UpdateAsync(answer);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}