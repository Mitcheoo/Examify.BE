// Examify.Application/Cqrs/Queries/Exercises/GetListeningExamQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetListeningExamQueryHandler : IRequestHandler<GetListeningExamQuery, ListeningExamDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetListeningExamQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ListeningExamDto> Handle(GetListeningExamQuery request, CancellationToken cancellationToken)
    {
        // Lấy thông tin bài thi
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found");

        // ✅ THÊM !q.IsDeleted
        var questions = await _unitOfWork.ListeningQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted);

        var questionList = questions.ToList();

        // Nhóm câu hỏi theo Part
        var parts = questionList
            .GroupBy(q => q.PartNumber)
            .OrderBy(g => g.Key)
            .Select(g => new ListeningPartDto
            {
                PartNumber = g.Key,
                AudioUrl = g.First().AudioUrl,
                Questions = _mapper.Map<List<ListeningQuestionDto>>(g.OrderBy(q => q.OrderNumber))
            }).ToList();

        return new ListeningExamDto
        {
            ExerciseId = exercise.Id,
            Title = exercise.Title,
            AudioUrl = exercise.AudioUrl,
            TimeLimitSeconds = exercise.TimeLimitSeconds,
            TotalQuestions = questionList.Count,
            TotalParts = parts.Count,
            Parts = parts,
            Questions = _mapper.Map<List<ListeningQuestionDto>>(questionList.OrderBy(q => q.OrderNumber))
        };
    }
}