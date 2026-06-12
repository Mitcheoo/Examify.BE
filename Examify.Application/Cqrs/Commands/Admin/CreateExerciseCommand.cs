// Examify.Application/Cqrs/Commands/Admin/CreateExerciseCommand.cs
using MediatR;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin;

public record CreateExerciseCommand(CreateExerciseDto Dto) : IRequest<ExerciseDto>;

public record UpdateExerciseCommand(Guid Id, UpdateExerciseDto Dto) : IRequest<ExerciseDto>;

public record DeleteExerciseCommand(Guid Id) : IRequest<bool>;

// Parts
public record CreatePartCommand(Guid ExerciseId, CreatePartDto Dto) : IRequest<PartDto>;

public record UpdatePartCommand(Guid ExerciseId, int PartNumber, UpdatePartDto Dto) : IRequest<PartDto>;

public record DeletePartCommand(Guid ExerciseId, int PartNumber) : IRequest<bool>;

// Reading Questions
public record CreateReadingQuestionCommand(Guid ExerciseId, CreateReadingQuestionDto Dto) : IRequest<ReadingQuestionDto>;

public record UpdateReadingQuestionCommand(Guid ExerciseId, Guid QuestionId, UpdateReadingQuestionDto Dto) : IRequest<ReadingQuestionDto>;

public record DeleteReadingQuestionCommand(Guid ExerciseId, Guid QuestionId) : IRequest<bool>;

// Listening Questions
public record CreateListeningQuestionCommand(Guid ExerciseId, CreateListeningQuestionDto Dto) : IRequest<ListeningQuestionDto>;

public record UpdateListeningQuestionCommand(Guid ExerciseId, Guid QuestionId, UpdateListeningQuestionDto Dto) : IRequest<ListeningQuestionDto>;

public record DeleteListeningQuestionCommand(Guid ExerciseId, Guid QuestionId) : IRequest<bool>;

// Writing Questions
public record CreateWritingQuestionCommand(Guid ExerciseId, CreateWritingQuestionDto Dto) : IRequest<WritingQuestionDto>;

public record UpdateWritingQuestionCommand(Guid ExerciseId, Guid QuestionId, UpdateWritingQuestionDto Dto) : IRequest<WritingQuestionDto>;

public record DeleteWritingQuestionCommand(Guid ExerciseId, Guid QuestionId) : IRequest<bool>;

// Speaking Questions
public record CreateSpeakingQuestionCommand(Guid ExerciseId, CreateSpeakingQuestionDto Dto) : IRequest<SpeakingQuestionDto>;

public record UpdateSpeakingQuestionCommand(Guid ExerciseId, Guid QuestionId, UpdateSpeakingQuestionDto Dto) : IRequest<SpeakingQuestionDto>;

public record DeleteSpeakingQuestionCommand(Guid ExerciseId, Guid QuestionId) : IRequest<bool>;