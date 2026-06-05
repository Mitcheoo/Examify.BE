using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Exercises;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        // Lấy Parts từ database
        var parts = await _unitOfWork.Parts
            .FindAsync(p => p.ExerciseId == request.ExerciseId);

        // Lấy Questions từ database
        var questions = await _unitOfWork.ReadingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId);

        var partList = parts.ToList();
        var questionList = questions.ToList();

        Console.WriteLine($"📖 Found {partList.Count} parts for exercise {request.ExerciseId}");
        Console.WriteLine($"📖 Found {questionList.Count} questions for exercise {request.ExerciseId}");

        return new ReadingExamDto
        {
            ExerciseId = exercise.Id,
            Title = exercise.Title,
            TimeLimitSeconds = exercise.TimeLimitSeconds,
            TotalQuestions = questionList.Count,
            Parts = _mapper.Map<List<PartDto>>(partList.OrderBy(p => p.PartNumber)),
            Questions = _mapper.Map<List<ReadingQuestionDto>>(questionList.OrderBy(q => q.OrderNumber))
        };
    }
}