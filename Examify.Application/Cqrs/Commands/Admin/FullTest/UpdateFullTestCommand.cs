// Examify.Application/Cqrs/Commands/Admin/FullTest/UpdateFullTestCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.FullTest;

public record UpdateFullTestCommand(Guid Id, UpdateFullTestDto Dto) : IRequest<ExerciseDto>;    