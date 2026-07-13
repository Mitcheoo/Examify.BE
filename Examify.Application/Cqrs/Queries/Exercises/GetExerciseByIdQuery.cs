// Examify.Application/Cqrs/Queries/Exercises/GetExerciseByIdQuery.cs
using MediatR;
using Examify.Core.Entities;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetExerciseByIdQuery(Guid Id) : IRequest<Exercise?>;