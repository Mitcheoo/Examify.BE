// Examify.Application/Cqrs/Commands/Admin/Exercise/CreateExerciseCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Exercise;

public record CreateExerciseCommand(CreateExerciseDto Dto) : IRequest<ExerciseDto>;