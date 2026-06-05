// Examify.Application/Cqrs/Queries/Exercises/GetExerciseResultSummaryQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetExerciseResultSummaryQuery(Guid ExerciseId, Guid UserId)
    : IRequest<ExerciseResultSummaryDto>;