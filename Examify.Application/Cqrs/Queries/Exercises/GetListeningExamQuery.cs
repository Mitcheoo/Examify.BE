// Examify.Application/Cqrs/Queries/Exercises/GetListeningExamQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetListeningExamQuery(Guid ExerciseId) : IRequest<ListeningExamDto>;