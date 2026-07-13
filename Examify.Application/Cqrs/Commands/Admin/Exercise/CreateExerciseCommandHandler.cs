// Examify.Application/Cqrs/Commands/Admin/Exercise/CreateExerciseCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core;
using Examify.Core.Entities;  // ✅ THÊM DÒNG NÀY - Chứa class Exercise
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Exercise;

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
        
        var exercise = new Core.Entities.Exercise {
            Id = Guid.NewGuid(),
            Skill = request.Dto.Skill,
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            TotalParts = request.Dto.TotalParts,
            TotalQuestions = request.Dto.TotalQuestions,
            TimeLimitSeconds = request.Dto.TimeLimitSeconds,
            Difficulty = request.Dto.Difficulty,
            IsFullTest = false,
            AttemptCount = 0,
            Source = request.Dto.Source,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Exercises.AddAsync(exercise);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExerciseDto>(exercise);
    }
}