// Examify.Application/Cqrs/Commands/Admin/CreateExerciseCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin;

public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, ExerciseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateExerciseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExerciseDto> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = _mapper.Map<Exercise>(request.Dto);
        exercise.Id = Guid.NewGuid();
        exercise.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Exercises.AddAsync(exercise);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExerciseDto>(exercise);
    }
}

public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand, ExerciseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateExerciseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExerciseDto> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.Id);
        if (exercise == null)
            throw new Exception("Exercise not found");

        _mapper.Map(request.Dto, exercise);
        exercise.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Exercises.UpdateAsync(exercise);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExerciseDto>(exercise);
    }
}

public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExerciseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.Id);
        if (exercise == null)
            throw new Exception("Exercise not found");

        exercise.IsDeleted = true;
        await _unitOfWork.Exercises.UpdateAsync(exercise);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

// Part Handlers
public class CreatePartCommandHandler : IRequestHandler<CreatePartCommand, PartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePartCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PartDto> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new Exception("Exercise not found");

        var part = new Part
        {
            ExerciseId = request.ExerciseId,
            PartNumber = request.Dto.PartNumber,
            Title = request.Dto.Title,
            Passage = request.Dto.Passage,
            AudioUrl = request.Dto.AudioUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Parts.AddAsync(part);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PartDto>(part);
    }
}

public class UpdatePartCommandHandler : IRequestHandler<UpdatePartCommand, PartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePartCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PartDto> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
    {
        var parts = await _unitOfWork.Parts
            .FindAsync(p => p.ExerciseId == request.ExerciseId && p.PartNumber == request.PartNumber);

        var part = parts.FirstOrDefault();
        if (part == null)
            throw new Exception("Part not found");

        if (request.Dto.Title != null) part.Title = request.Dto.Title;
        if (request.Dto.Passage != null) part.Passage = request.Dto.Passage;
        if (request.Dto.AudioUrl != null) part.AudioUrl = request.Dto.AudioUrl;
        part.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Parts.UpdateAsync(part);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PartDto>(part);
    }
}

public class DeletePartCommandHandler : IRequestHandler<DeletePartCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePartCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePartCommand request, CancellationToken cancellationToken)
    {
        var parts = await _unitOfWork.Parts
            .FindAsync(p => p.ExerciseId == request.ExerciseId && p.PartNumber == request.PartNumber);

        var part = parts.FirstOrDefault();
        if (part == null)
            throw new Exception("Part not found");

        part.IsDeleted = true;
        await _unitOfWork.Parts.UpdateAsync(part);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

// Reading Question Handlers
public class CreateReadingQuestionCommandHandler : IRequestHandler<CreateReadingQuestionCommand, ReadingQuestionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateReadingQuestionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReadingQuestionDto> Handle(CreateReadingQuestionCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new Exception("Exercise not found");

        var question = new ReadingQuestion
        {
            ExerciseId = request.ExerciseId,
            PartNumber = request.Dto.PartNumber,
            OrderNumber = request.Dto.OrderNumber,
            QuestionText = request.Dto.QuestionText,
            QuestionType = request.Dto.QuestionType,
            OptionsJson = request.Dto.OptionsJson,
            CorrectAnswer = request.Dto.CorrectAnswer,
            Explanation = request.Dto.Explanation,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ReadingQuestions.AddAsync(question);

        // Cập nhật TotalQuestions
        var questions = await _unitOfWork.ReadingQuestions.FindAsync(q => q.ExerciseId == request.ExerciseId);
        exercise.TotalQuestions = questions.Count() + 1;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ReadingQuestionDto>(question);
    }
}

public class UpdateReadingQuestionCommandHandler : IRequestHandler<UpdateReadingQuestionCommand, ReadingQuestionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateReadingQuestionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReadingQuestionDto> Handle(UpdateReadingQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.ReadingQuestions.GetByIdAsync(request.QuestionId);
        if (question == null)
            throw new Exception("Question not found");

        _mapper.Map(request.Dto, question);
        question.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ReadingQuestions.UpdateAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ReadingQuestionDto>(question);
    }
}

public class DeleteReadingQuestionCommandHandler : IRequestHandler<DeleteReadingQuestionCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReadingQuestionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteReadingQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.ReadingQuestions.GetByIdAsync(request.QuestionId);
        if (question == null)
            throw new Exception("Question not found");

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);

        question.IsDeleted = true;
        await _unitOfWork.ReadingQuestions.UpdateAsync(question);

        // Cập nhật TotalQuestions
        if (exercise != null && exercise.TotalQuestions > 0)
        {
            exercise.TotalQuestions--;
            await _unitOfWork.Exercises.UpdateAsync(exercise);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

// Listening Question Handlers
public class CreateListeningQuestionCommandHandler : IRequestHandler<CreateListeningQuestionCommand, ListeningQuestionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateListeningQuestionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ListeningQuestionDto> Handle(CreateListeningQuestionCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new Exception("Exercise not found");

        var question = new ListeningQuestion
        {
            ExerciseId = request.ExerciseId,
            PartNumber = request.Dto.PartNumber,
            OrderNumber = request.Dto.OrderNumber,
            QuestionText = request.Dto.QuestionText,
            OptionA = request.Dto.OptionA,
            OptionB = request.Dto.OptionB,
            OptionC = request.Dto.OptionC,
            OptionD = request.Dto.OptionD,
            CorrectAnswer = request.Dto.CorrectAnswer,
            AudioUrl = request.Dto.AudioUrl,
            Explanation = request.Dto.Explanation,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ListeningQuestions.AddAsync(question);

        var questions = await _unitOfWork.ListeningQuestions.FindAsync(q => q.ExerciseId == request.ExerciseId);
        exercise.TotalQuestions = questions.Count() + 1;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ListeningQuestionDto>(question);
    }
}

public class UpdateListeningQuestionCommandHandler : IRequestHandler<UpdateListeningQuestionCommand, ListeningQuestionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateListeningQuestionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ListeningQuestionDto> Handle(UpdateListeningQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.ListeningQuestions.GetByIdAsync(request.QuestionId);
        if (question == null)
            throw new Exception("Question not found");

        _mapper.Map(request.Dto, question);
        question.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ListeningQuestions.UpdateAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ListeningQuestionDto>(question);
    }
}

public class DeleteListeningQuestionCommandHandler : IRequestHandler<DeleteListeningQuestionCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteListeningQuestionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteListeningQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.ListeningQuestions.GetByIdAsync(request.QuestionId);
        if (question == null)
            throw new Exception("Question not found");

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);

        question.IsDeleted = true;
        await _unitOfWork.ListeningQuestions.UpdateAsync(question);

        if (exercise != null && exercise.TotalQuestions > 0)
        {
            exercise.TotalQuestions--;
            await _unitOfWork.Exercises.UpdateAsync(exercise);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

// Writing Question Handlers
public class CreateWritingQuestionCommandHandler : IRequestHandler<CreateWritingQuestionCommand, WritingQuestionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateWritingQuestionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WritingQuestionDto> Handle(CreateWritingQuestionCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new Exception("Exercise not found");

        var question = new WritingQuestion
        {
            ExerciseId = request.ExerciseId,
            OrderNumber = request.Dto.OrderNumber,
            TaskType = request.Dto.TaskType,
            PromptText = request.Dto.PromptText,
            SampleImageUrl = request.Dto.SampleImageUrl,
            ModelAnswer = request.Dto.ModelAnswer,
            RubricJson = request.Dto.RubricJson,
            MinWords = request.Dto.MinWords,
            MaxWords = request.Dto.MaxWords,
            RecommendedTimeMinutes = request.Dto.RecommendedTimeMinutes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.WritingQuestions.AddAsync(question);

        var questions = await _unitOfWork.WritingQuestions.FindAsync(q => q.ExerciseId == request.ExerciseId);
        exercise.TotalQuestions = questions.Count() + 1;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WritingQuestionDto>(question);
    }
}

public class UpdateWritingQuestionCommandHandler : IRequestHandler<UpdateWritingQuestionCommand, WritingQuestionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateWritingQuestionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WritingQuestionDto> Handle(UpdateWritingQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.WritingQuestions.GetByIdAsync(request.QuestionId);
        if (question == null)
            throw new Exception("Question not found");

        _mapper.Map(request.Dto, question);
        question.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.WritingQuestions.UpdateAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<WritingQuestionDto>(question);
    }
}

public class DeleteWritingQuestionCommandHandler : IRequestHandler<DeleteWritingQuestionCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWritingQuestionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteWritingQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.WritingQuestions.GetByIdAsync(request.QuestionId);
        if (question == null)
            throw new Exception("Question not found");

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);

        question.IsDeleted = true;
        await _unitOfWork.WritingQuestions.UpdateAsync(question);

        if (exercise != null && exercise.TotalQuestions > 0)
        {
            exercise.TotalQuestions--;
            await _unitOfWork.Exercises.UpdateAsync(exercise);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

// Speaking Question Handlers
public class CreateSpeakingQuestionCommandHandler : IRequestHandler<CreateSpeakingQuestionCommand, SpeakingQuestionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSpeakingQuestionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SpeakingQuestionDto> Handle(CreateSpeakingQuestionCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new Exception("Exercise not found");

        var question = new SpeakingQuestion
        {
            ExerciseId = request.ExerciseId,
            PartNumber = request.Dto.PartNumber,
            OrderNumber = request.Dto.OrderNumber,
            QuestionText = request.Dto.QuestionText,
            AudioUrl = request.Dto.AudioUrl,
            PreparationTime = request.Dto.PreparationTime,
            SpeakingTime = request.Dto.SpeakingTime,
            SampleAnswer = request.Dto.SampleAnswer,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SpeakingQuestions.AddAsync(question);

        var questions = await _unitOfWork.SpeakingQuestions.FindAsync(q => q.ExerciseId == request.ExerciseId);
        exercise.TotalQuestions = questions.Count() + 1;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SpeakingQuestionDto>(question);
    }
}

public class UpdateSpeakingQuestionCommandHandler : IRequestHandler<UpdateSpeakingQuestionCommand, SpeakingQuestionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSpeakingQuestionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SpeakingQuestionDto> Handle(UpdateSpeakingQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.SpeakingQuestions.GetByIdAsync(request.QuestionId);
        if (question == null)
            throw new Exception("Question not found");

        _mapper.Map(request.Dto, question);
        question.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SpeakingQuestions.UpdateAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SpeakingQuestionDto>(question);
    }
}

public class DeleteSpeakingQuestionCommandHandler : IRequestHandler<DeleteSpeakingQuestionCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSpeakingQuestionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteSpeakingQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _unitOfWork.SpeakingQuestions.GetByIdAsync(request.QuestionId);
        if (question == null)
            throw new Exception("Question not found");

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);

        question.IsDeleted = true;
        await _unitOfWork.SpeakingQuestions.UpdateAsync(question);

        if (exercise != null && exercise.TotalQuestions > 0)
        {
            exercise.TotalQuestions--;
            await _unitOfWork.Exercises.UpdateAsync(exercise);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}