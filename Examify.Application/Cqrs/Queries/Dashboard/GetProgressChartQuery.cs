// Examify.Application/Cqrs/Queries/Dashboard/GetProgressChartQuery.cs
using MediatR;
using Examify.Application.DTOs.Dashboard;

namespace Examify.Application.Cqrs.Queries.Dashboard;

public record GetProgressChartQuery(Guid UserId, int Weeks = 8) : IRequest<ProgressChartDto>;