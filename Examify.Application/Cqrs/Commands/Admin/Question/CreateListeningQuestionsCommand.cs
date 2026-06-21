// Examify.Application/Cqrs/Commands/Admin/Question/CreateListeningQuestionsCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Question;

public record CreateListeningQuestionsCommand(
    Guid ExerciseId,
    CreateListeningQuestionsDto Dto
) : IRequest<List<ListeningQuestionDto>>;