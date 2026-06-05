// Examify.Application/Cqrs/Queries/Exercises/GetExerciseProgressQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetExerciseProgressQuery(Guid ExerciseId, Guid UserId)
    : IRequest<ExerciseProgressDto>;