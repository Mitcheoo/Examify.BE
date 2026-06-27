// Examify.Application/Cqrs/Commands/Admin/FullTest/CreateFullTestCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.FullTest;

public record CreateFullTestCommand(CreateFullTestDto Dto) : IRequest<ExerciseDto>;