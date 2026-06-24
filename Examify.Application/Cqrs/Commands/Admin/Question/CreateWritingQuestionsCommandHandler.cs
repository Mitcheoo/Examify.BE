// Examify.Application/Cqrs/Commands/Admin/Question/CreateWritingQuestionsCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Question;

public class CreateWritingQuestionsCommandHandler
    : IRequestHandler<CreateWritingQuestionsCommand, List<WritingQuestionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateWritingQuestionsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<WritingQuestionDto>> Handle(
        CreateWritingQuestionsCommand request,
        CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        if (exercise.Skill != 2)
            throw new BadRequestException("This exercise is not Writing");

        var createdQuestions = new List<WritingQuestion>();

        foreach (var dto in request.Dto.Questions)
        {
            var question = new WritingQuestion
            {
                Id = Guid.NewGuid(),
                ExerciseId = request.ExerciseId,
                TaskType = dto.TaskType,
                OrderNumber = dto.OrderNumber,
                PromptText = dto.PromptText,
                MinWords = dto.MinWords,
                MaxWords = dto.MaxWords,
                RecommendedTimeMinutes = dto.RecommendedTimeMinutes,
                SampleImageUrl = dto.SampleImageUrl,
                ModelAnswer = dto.ModelAnswer,
                RubricJson = dto.RubricJson,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.WritingQuestions.AddAsync(question);
            createdQuestions.Add(question);
        }

        var totalQuestions = (await _unitOfWork.WritingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted))
            .Count();
        exercise.TotalQuestions = totalQuestions;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<List<WritingQuestionDto>>(createdQuestions);
    }
}