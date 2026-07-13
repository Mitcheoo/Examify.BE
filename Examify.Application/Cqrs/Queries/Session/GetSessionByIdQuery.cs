using MediatR;
using Examify.Core.Entities;

namespace Examify.Application.Cqrs.Queries.Session;

public record GetSessionByIdQuery(Guid SessionId) : IRequest<FullTestSession>;