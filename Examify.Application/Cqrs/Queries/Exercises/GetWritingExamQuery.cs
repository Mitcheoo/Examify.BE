// GetWritingExamQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetWritingExamQuery(Guid ExerciseId) : IRequest<WritingExamDto>;