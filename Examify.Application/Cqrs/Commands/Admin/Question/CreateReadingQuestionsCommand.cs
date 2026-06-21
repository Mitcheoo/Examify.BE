// Examify.Application/Cqrs/Commands/Admin/Question/CreateReadingQuestionsCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Question;

public record CreateReadingQuestionsCommand(
    Guid ExerciseId,
    CreateReadingQuestionsDto Dto
) : IRequest<List<ReadingQuestionDto>>;