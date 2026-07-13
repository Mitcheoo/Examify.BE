// Examify.Application/Cqrs/Commands/Admin/Question/CreateListeningQuestionsCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.Question;

public class CreateListeningQuestionsCommandHandler : IRequestHandler<CreateListeningQuestionsCommand, List<ListeningQuestionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateListeningQuestionsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ListeningQuestionDto>> Handle(CreateListeningQuestionsCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        if (exercise.Skill != 1)
            throw new BadRequestException("This exercise is not Listening");

        var createdQuestions = new List<ListeningQuestion>();

        foreach (var dto in request.Dto.Questions)
        {
            var question = new ListeningQuestion
            {
                Id = Guid.NewGuid(),
                ExerciseId = request.ExerciseId,
                PartNumber = dto.PartNumber,
                OrderNumber = dto.OrderNumber,
                QuestionText = dto.QuestionText,
                OptionA = dto.OptionA,
                OptionB = dto.OptionB,
                OptionC = dto.OptionC,
                OptionD = dto.OptionD,
                CorrectAnswer = dto.CorrectAnswer,
                AudioUrl = dto.AudioUrl,
                Explanation = dto.Explanation,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.ListeningQuestions.AddAsync(question);
            createdQuestions.Add(question);
        }

        var totalQuestions = (await _unitOfWork.ListeningQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted))
            .Count();
        exercise.TotalQuestions = totalQuestions;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<List<ListeningQuestionDto>>(createdQuestions);
    }
}