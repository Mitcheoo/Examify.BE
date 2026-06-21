// Examify.Application/Cqrs/Queries/Exercises/GetExerciseStatsQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetExerciseStatsQuery(
    Guid UserId,
    int? Skill = null
) : IRequest<List<ExerciseStatsDto>>;