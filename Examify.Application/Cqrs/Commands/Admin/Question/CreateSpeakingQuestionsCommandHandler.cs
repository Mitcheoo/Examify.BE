// Examify.Application/Cqrs/Commands/Admin/Question/CreateSpeakingQuestionsCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Question;

public class CreateSpeakingQuestionsCommandHandler : IRequestHandler<CreateSpeakingQuestionsCommand, List<SpeakingQuestionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSpeakingQuestionsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<SpeakingQuestionDto>> Handle(CreateSpeakingQuestionsCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        if (exercise.Skill != 3)
            throw new BadRequestException("This exercise is not Speaking");

        var createdQuestions = new List<SpeakingQuestion>();

        foreach (var dto in request.Dto.Questions)
        {
            var question = new SpeakingQuestion
            {
                Id = Guid.NewGuid(),
                ExerciseId = request.ExerciseId,
                PartNumber = dto.PartNumber,
                OrderNumber = dto.OrderNumber,
                QuestionText = dto.QuestionText,
                PreparationTime = dto.PreparationTime,
                SpeakingTime = dto.SpeakingTime,
                SampleAnswer = dto.SampleAnswer,
                AudioUrl = dto.AudioUrl,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.SpeakingQuestions.AddAsync(question);
            createdQuestions.Add(question);
        }

        var totalQuestions = (await _unitOfWork.SpeakingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted))
            .Count();
        exercise.TotalQuestions = totalQuestions;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<List<SpeakingQuestionDto>>(createdQuestions);
    }
}