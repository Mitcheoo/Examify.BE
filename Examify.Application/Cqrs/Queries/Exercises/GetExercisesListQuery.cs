// Examify.Application/Cqrs/Queries/Exercises/GetExercisesListQuery.cs
using MediatR;
using Examify.Application.DTOs.Exercises;
using Examify.Core.Enums;

namespace Examify.Application.Cqrs.Queries.Exercises;

public record GetExercisesListQuery(SkillType? Skill = null) : IRequest<List<ExerciseDto>>;