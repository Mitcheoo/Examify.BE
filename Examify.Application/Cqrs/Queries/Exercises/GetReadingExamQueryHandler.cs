// Examify.Application/Cqrs/Queries/Exercises/GetReadingExamQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetReadingExamQueryHandler : IRequestHandler<GetReadingExamQuery, ReadingExamDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReadingExamQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReadingExamDto> Handle(GetReadingExamQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new Exception("Exercise not found");

        var questions = await _unitOfWork.ReadingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId);

        var parts = await _unitOfWork.Parts
            .FindAsync(p => p.ExerciseId == request.ExerciseId);

        return new ReadingExamDto
        {
            ExerciseId = exercise.Id,
            Title = exercise.Title,
            TimeLimitSeconds = exercise.TimeLimitSeconds,
            TotalQuestions = questions.Count(),
            Parts = _mapper.Map<List<PartDto>>(parts),
            Questions = _mapper.Map<List<ReadingQuestionDto>>(questions.OrderBy(q => q.OrderNumber))
        };
    }
}