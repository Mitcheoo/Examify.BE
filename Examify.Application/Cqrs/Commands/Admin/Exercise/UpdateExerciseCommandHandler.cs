// Examify.Application/Cqrs/Commands/Admin/Exercise/UpdateExerciseCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Exercise;

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
            throw new NotFoundException("Exercise not found");

        if (exercise.IsFullTest)
            throw new BadRequestException("Cannot update Full Test with Exercise API");

        exercise.Title = request.Dto.Title;
        exercise.Description = request.Dto.Description;
        exercise.TotalParts = request.Dto.TotalParts;
        exercise.TotalQuestions = request.Dto.TotalQuestions;
        exercise.TimeLimitSeconds = request.Dto.TimeLimitSeconds;
        exercise.Difficulty = request.Dto.Difficulty;
        exercise.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Exercises.UpdateAsync(exercise);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExerciseDto>(exercise);
    }
}