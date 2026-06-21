// Examify.Application/Cqrs/Queries/FullTest/GetFullTestStatusQuery.cs
using MediatR;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Queries.FullTest;

public record GetFullTestStatusQuery(Guid FullTestId, Guid UserId) : IRequest<FullTestStatusDto>;