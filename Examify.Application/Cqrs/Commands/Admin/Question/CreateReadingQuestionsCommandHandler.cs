// Examify.Application/Cqrs/Commands/Admin/Question/CreateReadingQuestionsCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;
using System.Text.Json;

namespace Examify.Application.Cqrs.Commands.Admin.Question;

public class CreateReadingQuestionsCommandHandler : IRequestHandler<CreateReadingQuestionsCommand, List<ReadingQuestionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateReadingQuestionsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ReadingQuestionDto>> Handle(CreateReadingQuestionsCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        if (exercise.Skill != 0)
            throw new BadRequestException("This exercise is not Reading");

        var createdQuestions = new List<ReadingQuestion>();

        foreach (var dto in request.Dto.Questions)
        {
            var question = new ReadingQuestion
            {
                Id = Guid.NewGuid(),
                ExerciseId = request.ExerciseId,
                PartNumber = dto.PartNumber,
                OrderNumber = dto.OrderNumber,
                QuestionType = dto.QuestionType,
                QuestionText = dto.QuestionText,
                OptionsJson = JsonSerializer.Serialize(dto.Options),
                CorrectAnswer = dto.CorrectAnswer,
                Explanation = dto.Explanation,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.ReadingQuestions.AddAsync(question);
            createdQuestions.Add(question);
        }

        // Cập nhật TotalQuestions
        var totalQuestions = (await _unitOfWork.ReadingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted))
            .Count();
        exercise.TotalQuestions = totalQuestions;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<List<ReadingQuestionDto>>(createdQuestions);
    }
}