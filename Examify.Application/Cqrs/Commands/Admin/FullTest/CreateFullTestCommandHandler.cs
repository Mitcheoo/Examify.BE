// Examify.Application/Cqrs/Commands/Admin/FullTest/CreateFullTestCommandHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Commands.Admin.FullTest;

public class CreateFullTestCommandHandler : IRequestHandler<CreateFullTestCommand, ExerciseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateFullTestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExerciseDto> Handle(CreateFullTestCommand request, CancellationToken cancellationToken)
    {
        // ============================================================
        // BƯỚC 1: TẠO 4 KỸ NĂNG CON (NẾU CHƯA CÓ)
        // ============================================================

        // Lấy ID từ request (nếu có)
        var readingId = request.Dto.ReadingExerciseId;
        var listeningId = request.Dto.ListeningExerciseId;
        var writingId = request.Dto.WritingExerciseId;
        var speakingId = request.Dto.SpeakingExerciseId;

        // ✅ NẾU KHÔNG CÓ, TỰ TẠO
        if (!readingId.HasValue)
            readingId = await CreateSkillExercise(0, "Reading");

        if (!listeningId.HasValue)
            listeningId = await CreateSkillExercise(1, "Listening");

        if (!writingId.HasValue)
            writingId = await CreateSkillExercise(2, "Writing");

        if (!speakingId.HasValue)
            speakingId = await CreateSkillExercise(3, "Speaking");

        // ============================================================
        // BƯỚC 2: TÍNH TỔNG SỐ CÂU HỎI
        // ============================================================
        int totalQuestions = 0;

        if (readingId.HasValue)
        {
            var reading = await _unitOfWork.Exercises.GetByIdAsync(readingId.Value);
            if (reading != null) totalQuestions += reading.TotalQuestions;
        }
        if (listeningId.HasValue)
        {
            var listening = await _unitOfWork.Exercises.GetByIdAsync(listeningId.Value);
            if (listening != null) totalQuestions += listening.TotalQuestions;
        }
        if (writingId.HasValue)
        {
            var writing = await _unitOfWork.Exercises.GetByIdAsync(writingId.Value);
            if (writing != null) totalQuestions += writing.TotalQuestions;
        }
        if (speakingId.HasValue)
        {
            var speaking = await _unitOfWork.Exercises.GetByIdAsync(speakingId.Value);
            if (speaking != null) totalQuestions += speaking.TotalQuestions;
        }

        // ============================================================
        // BƯỚC 3: TẠO FULL TEST - ✅ THÊM IsFree VÀ Price
        // ============================================================
        var fullTest = new Examify.Core.Entities.Exercise
        {
            Id = Guid.NewGuid(),
            Skill = 4,
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            TotalParts = 4,
            TotalQuestions = totalQuestions,
            TimeLimitSeconds = request.Dto.TimeLimitSeconds,
            Difficulty = request.Dto.Difficulty,
            IsFullTest = true,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,

            // ✅ THÊM 2 DÒNG NÀY
            IsFree = request.Dto.IsFree,
            Price = request.Dto.Price,

            ReadingExerciseId = readingId,
            ListeningExerciseId = listeningId,
            WritingExerciseId = writingId,
            SpeakingExerciseId = speakingId
        };

        await _unitOfWork.Exercises.AddAsync(fullTest);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExerciseDto>(fullTest);
    }

    /// <summary>
    /// Tạo bài thi kỹ năng con
    /// </summary>
    private async Task<Guid> CreateSkillExercise(int skill, string skillName)
    {
        var exercise = new Examify.Core.Entities.Exercise
        {
            Id = Guid.NewGuid(),
            Skill = skill,
            Title = $"VSTEP {skillName} Test - {DateTime.Now:yyyy-MM-dd HH:mm}",
            Description = $"Bài thi {skillName} cho Full Test",
            IsFullTest = false,
            TotalParts = skill == 2 ? 1 : 3,
            TotalQuestions = 3,
            TimeLimitSeconds = skill switch
            {
                0 => 3600,   // Reading: 60 phút
                1 => 2400,   // Listening: 40 phút
                2 => 3600,   // Writing: 60 phút
                3 => 1800,   // Speaking: 30 phút
                _ => 3600
            },
            Difficulty = 2,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,

            // ✅ THÊM CHO SKILL CON (MẶC ĐỊNH MIỄN PHÍ)
            IsFree = true,
            Price = 0
        };

        await _unitOfWork.Exercises.AddAsync(exercise);
        await _unitOfWork.SaveChangesAsync();

        return exercise.Id;
    }
}