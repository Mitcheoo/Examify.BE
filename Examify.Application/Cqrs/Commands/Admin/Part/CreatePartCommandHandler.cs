// Examify.Application/Cqrs/Commands/Admin/Part/CreatePartCommandHandler.cs

using AutoMapper;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;
using Examify.Core.Entities;  // ✅ Đảm bảo có using này
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Commands.Admin.Part;  // ⚠️ Namespace này có tên "Part"

public class CreatePartCommandHandler : IRequestHandler<CreatePartCommand, PartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePartCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PartDto> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null || exercise.IsDeleted)
        {
            throw new NotFoundException($"Không tìm thấy Exercise với ID '{request.ExerciseId}'");
        }

        // ✅ SỬA: Dùng full path để tránh conflict với namespace
        var existingParts = await _unitOfWork.Parts
            .FindAsync(p => p.ExerciseId == request.ExerciseId && p.PartNumber == request.Dto.PartNumber && !p.IsDeleted);

        if (existingParts.Any())
        {
            var existingPart = existingParts.First();
            existingPart.Passage = request.Dto.Passage;
            existingPart.Title = request.Dto.Title ?? $"Part {request.Dto.PartNumber}";
            existingPart.AudioUrl = request.Dto.AudioUrl;
            existingPart.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Parts.UpdateAsync(existingPart);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PartDto>(existingPart);
        }

        // ✅ SỬA: Dùng full path Examify.Core.Entities.Part
        var part = new Examify.Core.Entities.Part  // ← Dùng full path
        {
            Id = Guid.NewGuid(),
            ExerciseId = request.ExerciseId,
            PartNumber = request.Dto.PartNumber,
            Title = request.Dto.Title ?? $"Part {request.Dto.PartNumber}",
            Passage = request.Dto.Passage,
            AudioUrl = request.Dto.AudioUrl,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Parts.AddAsync(part);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PartDto>(part);
    }
}