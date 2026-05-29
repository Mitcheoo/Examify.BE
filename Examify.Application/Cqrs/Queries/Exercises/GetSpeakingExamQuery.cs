// Examify.Application/Cqrs/Queries/Exercises/GetSpeakingExamQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetSpeakingExamQuery(Guid ExerciseId) : IRequest<SpeakingExamDto>;