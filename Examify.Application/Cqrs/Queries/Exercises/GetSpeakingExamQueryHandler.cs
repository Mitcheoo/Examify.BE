// Examify.Application/Cqrs/Queries/Exercises/GetSpeakingExamQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetSpeakingExamQueryHandler : IRequestHandler<GetSpeakingExamQuery, SpeakingExamDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSpeakingExamQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SpeakingExamDto> Handle(GetSpeakingExamQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        var questions = await _unitOfWork.SpeakingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId);

        var questionList = questions.ToList();

        var parts = questionList
            .GroupBy(q => q.PartNumber)
            .OrderBy(g => g.Key)
            .Select(g => new SpeakingPartDto
            {
                PartNumber = g.Key,
                PartTitle = $"PART {g.Key}",
                PreparationTime = g.First().PreparationTime,
                SpeakingTime = g.First().SpeakingTime,
                Questions = _mapper.Map<List<SpeakingQuestionDto>>(g.OrderBy(q => q.OrderNumber))
            }).ToList();

        return new SpeakingExamDto
        {
            ExerciseId = exercise.Id,
            Title = exercise.Title,
            TimeLimitSeconds = exercise.TimeLimitSeconds,
            TotalQuestions = questionList.Count,
            Parts = parts,
            Questions = _mapper.Map<List<SpeakingQuestionDto>>(questionList.OrderBy(q => q.OrderNumber))
        };
    }
}