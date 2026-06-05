// Examify.Application/Cqrs/Queries/Exercises/GetExerciseDetailQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetExerciseDetailQuery(Guid ExerciseId) : IRequest<ExerciseDetailDto>;