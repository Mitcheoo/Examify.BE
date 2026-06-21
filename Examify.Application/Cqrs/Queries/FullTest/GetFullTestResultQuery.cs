// Examify.Application/Cqrs/Queries/FullTest/GetFullTestResultQuery.cs
using MediatR;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Queries.FullTest;

public record GetFullTestResultQuery(Guid FullTestId, Guid UserId) : IRequest<FullTestResultDetailDto>;