// Examify.Application/Cqrs/Queries/Dashboard/GetDashboardStatsQuery.cs
using MediatR;
using Examify.Application.DTOs.Dashboard;

namespace Examify.Application.Cqrs.Queries.Dashboard;

public record GetDashboardStatsQuery(Guid UserId) : IRequest<DashboardStatsDto>;