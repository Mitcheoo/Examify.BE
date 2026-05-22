// Examify.Application/Cqrs/Queries/Exercises/GetExerciseQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetExerciseQuery(Guid Id) : IRequest<ExerciseDto>;