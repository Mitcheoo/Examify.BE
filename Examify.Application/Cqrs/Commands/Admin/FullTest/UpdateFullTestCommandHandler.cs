// Examify.Application/Cqrs/Commands/Admin/FullTest/UpdateFullTestCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.FullTest;

public class UpdateFullTestCommandHandler : IRequestHandler<UpdateFullTestCommand, ExerciseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateFullTestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExerciseDto> Handle(UpdateFullTestCommand request, CancellationToken cancellationToken)
    {
        var fullTest = await _unitOfWork.Exercises.GetByIdAsync(request.Id);
        if (fullTest == null)
            throw new NotFoundException("Full Test not found");

        if (!fullTest.IsFullTest)
            throw new BadRequestException("This exercise is not a Full Test");

        fullTest.Title = request.Dto.Title;
        fullTest.Description = request.Dto.Description;
        fullTest.TimeLimitSeconds = request.Dto.TimeLimitSeconds;
        fullTest.Difficulty = request.Dto.Difficulty;
        fullTest.UpdatedAt = DateTime.UtcNow;

        // Cập nhật các ID liên kết (nếu có)
        if (request.Dto.ReadingExerciseId.HasValue)
            fullTest.ReadingExerciseId = request.Dto.ReadingExerciseId.Value;

        if (request.Dto.ListeningExerciseId.HasValue)
            fullTest.ListeningExerciseId = request.Dto.ListeningExerciseId.Value;

        if (request.Dto.WritingExerciseId.HasValue)
            fullTest.WritingExerciseId = request.Dto.WritingExerciseId.Value;

        if (request.Dto.SpeakingExerciseId.HasValue)
            fullTest.SpeakingExerciseId = request.Dto.SpeakingExerciseId.Value;

        // Cập nhật tổng số câu hỏi
        if (fullTest.ReadingExerciseId.HasValue)
        {
            var reading = await _unitOfWork.Exercises.GetByIdAsync(fullTest.ReadingExerciseId.Value);
            if (reading != null)
                fullTest.TotalQuestions += reading.TotalQuestions;
        }
        if (fullTest.ListeningExerciseId.HasValue)
        {
            var listening = await _unitOfWork.Exercises.GetByIdAsync(fullTest.ListeningExerciseId.Value);
            if (listening != null)
                fullTest.TotalQuestions += listening.TotalQuestions;
        }
        if (fullTest.WritingExerciseId.HasValue)
        {
            var writing = await _unitOfWork.Exercises.GetByIdAsync(fullTest.WritingExerciseId.Value);
            if (writing != null)
                fullTest.TotalQuestions += writing.TotalQuestions;
        }
        if (fullTest.SpeakingExerciseId.HasValue)
        {
            var speaking = await _unitOfWork.Exercises.GetByIdAsync(fullTest.SpeakingExerciseId.Value);
            if (speaking != null)
                fullTest.TotalQuestions += speaking.TotalQuestions;
        }

        await _unitOfWork.Exercises.UpdateAsync(fullTest);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExerciseDto>(fullTest);
    }
}