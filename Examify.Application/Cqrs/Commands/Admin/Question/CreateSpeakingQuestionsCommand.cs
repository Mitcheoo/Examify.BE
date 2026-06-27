// Examify.Application/Cqrs/Commands/Admin/Question/CreateSpeakingQuestionsCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Question;

public record CreateSpeakingQuestionsCommand(
    Guid ExerciseId,
    CreateSpeakingQuestionsDto Dto
) : IRequest<List<SpeakingQuestionDto>>;