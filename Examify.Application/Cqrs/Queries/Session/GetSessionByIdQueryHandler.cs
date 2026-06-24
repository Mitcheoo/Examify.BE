// Examify.Application/Cqrs/Queries/Session/GetSessionByIdQueryHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;

namespace Examify.Application.Cqrs.Queries.Session;

public sealed class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, FullTestSession>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSessionByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FullTestSession> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId);
        if (session is null)
            throw new NotFoundException($"Session with ID {request.SessionId} not found");

        return session;
    }
}