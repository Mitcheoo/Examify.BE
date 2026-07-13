// Examify.Application/Cqrs/Queries/Session/GetAnswersQuery.cs
using MediatR;
using Examify.Application.DTOs.Session;

namespace Examify.Application.Cqrs.Queries.Session;

public record GetAnswersQuery(Guid SessionId) : IRequest<List<SessionAnswerDto>>;