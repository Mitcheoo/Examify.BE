// Examify.Application/Cqrs/Commands/Admin/FullTest/CreateFullTestCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.FullTest;

public class CreateFullTestCommandHandler : IRequestHandler<CreateFullTestCommand, ExerciseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateFullTestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExerciseDto> Handle(CreateFullTestCommand request, CancellationToken cancellationToken)
    {
        // ✅ LẤY ID TỪ REQUEST
        var readingId = request.Dto.ReadingExerciseId;
        var listeningId = request.Dto.ListeningExerciseId;
        var writingId = request.Dto.WritingExerciseId;
        var speakingId = request.Dto.SpeakingExerciseId;

        // ✅ TÍNH TỔNG SỐ CÂU HỎI TỪ CÁC BÀI CON
        int totalQuestions = 0;

        if (readingId.HasValue)
        {
            var reading = await _unitOfWork.Exercises.GetByIdAsync(readingId.Value);
            if (reading != null) totalQuestions += reading.TotalQuestions;
        }
        if (listeningId.HasValue)
        {
            var listening = await _unitOfWork.Exercises.GetByIdAsync(listeningId.Value);
            if (listening != null) totalQuestions += listening.TotalQuestions;
        }
        if (writingId.HasValue)
        {
            var writing = await _unitOfWork.Exercises.GetByIdAsync(writingId.Value);
            if (writing != null) totalQuestions += writing.TotalQuestions;
        }
        if (speakingId.HasValue)
        {
            var speaking = await _unitOfWork.Exercises.GetByIdAsync(speakingId.Value);
            if (speaking != null) totalQuestions += speaking.TotalQuestions;
        }

        var fullTest = new Core.Entities.Exercise
        {
            Id = Guid.NewGuid(),
            Skill = 4,
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            TotalParts = 4,
            TotalQuestions = totalQuestions,  // ✅ TÍNH TỔNG
            TimeLimitSeconds = request.Dto.TimeLimitSeconds,
            Difficulty = request.Dto.Difficulty,
            IsFullTest = true,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,

            // ✅ GÁN ID TỪ REQUEST
            ReadingExerciseId = readingId,
            ListeningExerciseId = listeningId,
            WritingExerciseId = writingId,
            SpeakingExerciseId = speakingId
        };

        await _unitOfWork.Exercises.AddAsync(fullTest);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExerciseDto>(fullTest);
    }
}