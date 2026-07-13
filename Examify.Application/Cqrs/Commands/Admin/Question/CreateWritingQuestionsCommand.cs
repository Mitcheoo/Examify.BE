// Examify.Application/Cqrs/Commands/Admin/Question/CreateWritingQuestionsCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Question;

public record CreateWritingQuestionsCommand(
    Guid ExerciseId,
    CreateWritingQuestionsDto Dto
) : IRequest<List<WritingQuestionDto>>;