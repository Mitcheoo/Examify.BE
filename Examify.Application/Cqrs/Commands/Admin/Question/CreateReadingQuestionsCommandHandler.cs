// Examify.Application/Cqrs/Commands/Admin/Question/CreateReadingQuestionsCommandHandler.cs

using AutoMapper;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;
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
        // Kiểm tra Exercise tồn tại
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null || exercise.IsDeleted)
        {
            throw new NotFoundException($"Không tìm thấy Exercise với ID '{request.ExerciseId}'");
        }

        var createdQuestions = new List<ReadingQuestion>();

        foreach (var dto in request.Dto.Questions)
        {
            // ✅ SỬA: Dùng full path hoặc alias
            var question = new Examify.Core.Entities.ReadingQuestion  // ← Dùng full path
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

            // ✅ NẾU CÓ PASSAGE, LƯU VÀO BẢNG Parts
            if (!string.IsNullOrEmpty(dto.Passage))
            {
                // ✅ SỬA: Dùng full path cho Part
                var existingParts = await _unitOfWork.Parts
                    .FindAsync(p => p.ExerciseId == request.ExerciseId && p.PartNumber == dto.PartNumber && !p.IsDeleted);

                if (existingParts.Any())
                {
                    // Cập nhật Passage nếu Part đã tồn tại
                    var existingPart = existingParts.First();
                    existingPart.Passage = dto.Passage;
                    existingPart.Title = $"Part {dto.PartNumber}";
                    existingPart.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Parts.UpdateAsync(existingPart);
                }
                else
                {
                    // Tạo mới Part nếu chưa có
                    var newPart = new Examify.Core.Entities.Part  // ← Dùng full path
                    {
                        Id = Guid.NewGuid(),
                        ExerciseId = request.ExerciseId,
                        PartNumber = dto.PartNumber,
                        Title = $"Part {dto.PartNumber}",
                        Passage = dto.Passage,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _unitOfWork.Parts.AddAsync(newPart);
                }
            }

            await _unitOfWork.ReadingQuestions.AddAsync(question);
            createdQuestions.Add(question);
        }

        // Cập nhật số lượng câu hỏi cho Exercise
        var allQuestions = await _unitOfWork.ReadingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted);
        exercise.TotalQuestions = allQuestions.Count();
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<List<ReadingQuestionDto>>(createdQuestions);
    }
}