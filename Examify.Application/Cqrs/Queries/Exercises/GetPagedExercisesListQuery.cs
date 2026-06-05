// Examify.Application/Cqrs/Queries/Exercises/GetPagedExercisesListQuery.cs
using MediatR;
using Examify.Application.DTOs.Common;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetPagedExercisesListQuery(
    int Page,
    int PageSize,
    int? Skill,
    string? Search
) : IRequest<PagedResult<ExerciseDto>>;