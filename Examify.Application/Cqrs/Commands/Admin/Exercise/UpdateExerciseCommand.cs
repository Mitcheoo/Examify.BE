// Examify.Application/Cqrs/Commands/Admin/Exercise/UpdateExerciseCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Exercise;

public record UpdateExerciseCommand(Guid Id, UpdateExerciseDto Dto) : IRequest<ExerciseDto>;