// Examify.Application/Cqrs/Queries/Exercises/GetReadingExamQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetReadingExamQuery(Guid ExerciseId) : IRequest<ReadingExamDto>;