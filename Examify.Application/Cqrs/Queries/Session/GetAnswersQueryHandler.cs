// Examify.Application/Cqrs/Queries/Session/GetAnswersQueryHandler.cs
using AutoMapper;
using Examify.Application.DTOs.Session;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Session;

public sealed class GetAnswersQueryHandler : IRequestHandler<GetAnswersQuery, List<SessionAnswerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAnswersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<SessionAnswerDto>> Handle(GetAnswersQuery request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId);
        if (session is null)
            throw new NotFoundException("Session not found");

        var answers = await _unitOfWork.SessionAnswers
            .FindAsync(a => a.SessionId == request.SessionId && !a.IsSubmitted);

        return _mapper.Map<List<SessionAnswerDto>>(answers.OrderBy(a => a.UpdatedAt));
    }
}