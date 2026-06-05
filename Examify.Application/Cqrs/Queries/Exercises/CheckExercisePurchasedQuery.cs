// Examify.Application/Cqrs/Queries/Exercises/CheckExercisePurchasedQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record CheckExercisePurchasedQuery(Guid ExerciseId, Guid UserId)
    : IRequest<PurchasedStatusDto>;