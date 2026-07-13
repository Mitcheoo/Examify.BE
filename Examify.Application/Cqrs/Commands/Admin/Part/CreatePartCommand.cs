// Examify.Application/Cqrs/Commands/Admin/Part/CreatePartCommand.cs
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Admin.Part;

public class CreatePartCommand : IRequest<PartDto>
{
    public Guid ExerciseId { get; set; }
    public CreatePartDto Dto { get; set; }

    public CreatePartCommand(Guid exerciseId, CreatePartDto dto)
    {
        ExerciseId = exerciseId;
        Dto = dto;
    }
}