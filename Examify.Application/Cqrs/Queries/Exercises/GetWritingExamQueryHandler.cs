// GetWritingExamQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetWritingExamQueryHandler : IRequestHandler<GetWritingExamQuery, WritingExamDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetWritingExamQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WritingExamDto> Handle(GetWritingExamQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        var questions = await _unitOfWork.WritingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId);

        return new WritingExamDto
        {
            ExerciseId = exercise.Id,
            Title = exercise.Title,
            TimeLimitSeconds = exercise.TimeLimitSeconds,
            Questions = _mapper.Map<List<WritingQuestionDto>>(questions.OrderBy(q => q.OrderNumber))
        };
    }
}