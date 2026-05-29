// Examify.Application/Cqrs/Queries/Exercises/GetExercisesListQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetExercisesListQuery() : IRequest<List<ExerciseDto>>;