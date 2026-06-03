// Examify.Application/Cqrs/Queries/Dashboard/GetWeaknessAnalysisQuery.cs
using MediatR;
using Examify.Application.DTOs.Dashboard;

namespace Examify.Application.Cqrs.Queries.Dashboard;

public record GetWeaknessAnalysisQuery(Guid UserId) : IRequest<WeaknessAnalysisDto>;