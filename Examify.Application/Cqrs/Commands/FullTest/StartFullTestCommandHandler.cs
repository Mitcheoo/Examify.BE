// Examify.Application/Cqrs/Commands/FullTest/StartFullTestCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.FullTest;

public class StartFullTestCommandHandler : IRequestHandler<StartFullTestCommand, StartFullTestResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public StartFullTestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StartFullTestResponse> Handle(StartFullTestCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy Full Test
        var fullTest = await _unitOfWork.Exercises.GetByIdAsync(request.FullTestId);
        if (fullTest is null || !fullTest.IsFullTest)
            throw new NotFoundException($"Full Test with ID {request.   FullTestId} not found");

        // 2. Tạo hoặc lấy 4 Exercise con (CHỈ LIÊN KẾT VỚI FULL TEST NÀY)
        var readingId = await GetOrCreateChildExercise(fullTest.Id, 0, "Reading");
        var listeningId = await GetOrCreateChildExercise(fullTest.Id, 1, "Listening");
        var writingId = await GetOrCreateChildExercise(fullTest.Id, 2, "Writing");
        var speakingId = await GetOrCreateChildExercise(fullTest.Id, 3, "Speaking");

        // 3. Cập nhật Full Test với ExerciseId (nếu chưa có)
        bool isUpdated = false;
        if (!fullTest.ReadingExerciseId.HasValue && readingId.HasValue)
        {
            fullTest.ReadingExerciseId = readingId.Value;
            isUpdated = true;
        }
        if (!fullTest.ListeningExerciseId.HasValue && listeningId.HasValue)
        {
            fullTest.ListeningExerciseId = listeningId.Value;
            isUpdated = true;
        }
        if (!fullTest.WritingExerciseId.HasValue && writingId.HasValue)
        {
            fullTest.WritingExerciseId = writingId.Value;
            isUpdated = true;
        }
        if (!fullTest.SpeakingExerciseId.HasValue && speakingId.HasValue)
        {
            fullTest.SpeakingExerciseId = speakingId.Value;
            isUpdated = true;
        }

        if (isUpdated)
        {
            await _unitOfWork.Exercises.UpdateAsync(fullTest);
            await _unitOfWork.SaveChangesAsync();
        }

        // 4. Tạo Session
        var session = new FullTestSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            StartTime = DateTime.UtcNow,
            Status = 0,
            CurrentPart = 1,
            ReadingExerciseId = readingId,
            ListeningExerciseId = listeningId,
            WritingExerciseId = writingId,
            SpeakingExerciseId = speakingId,
            ReadingTimeSpent = 0,
            ListeningTimeSpent = 0,
            WritingTimeSpent = 0,
            SpeakingTimeSpent = 0,
            TotalScore = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.FullTestSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        // 5. Tạo response
        var parts = new List<FullTestPartDto>();
        var exerciseIds = new[] { readingId, listeningId, writingId, speakingId };
        var partNames = new[] { "Reading", "Listening", "Writing", "Speaking" };
        var timeLimits = new[] { 3600, 2100, 3600, 1020 };

        for (int i = 0; i < 4; i++)
        {
            if (exerciseIds[i].HasValue)
            {
                var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseIds[i].Value);
                parts.Add(new FullTestPartDto
                {
                    PartNumber = i + 1,
                    PartName = partNames[i],
                    ExerciseId = exerciseIds[i].Value,
                    TimeLimitSeconds = exercise?.TimeLimitSeconds ?? timeLimits[i],
                    TotalQuestions = exercise?.TotalQuestions ?? 3,
                    IsCompleted = false
                });
            }
        }

        return new StartFullTestResponse
        {
            SessionId = session.Id,
            StartTime = session.StartTime,
            Parts = parts
        };
    }

    /// <summary>
    /// Tạo Exercise con nếu chưa tồn tại, ngược lại trả về ID đã có
    /// </summary>
    private async Task<Guid?> GetOrCreateChildExercise(Guid fullTestId, int skill, string skillName)
    {
        // ✅ CHỈ TÌM EXERCISE CON CỦA FULL TEST NÀY
        var existing = await _unitOfWork.Exercises
            .FindAsync(e => e.FullTestId == fullTestId && e.Skill == skill && !e.IsDeleted);

        if (existing.Any())
            return existing.First().Id;

        // Tạo mới
        var newExercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Skill = skill,
            Title = $"VSTEP {skillName} Test - {DateTime.Now:yyyy-MM-dd HH:mm}",
            Description = $"Bài thi {skillName} cho Full Test",
            FullTestId = fullTestId,  // ✅ LIÊN KẾT VỚI FULL TEST
            IsFullTest = false,
            TotalParts = 1,
            TotalQuestions = 3,
            TimeLimitSeconds = skill switch
            {
                0 => 3600,   // Reading: 60 phút
                1 => 2100,   // Listening: 35 phút
                2 => 3600,   // Writing: 60 phút
                3 => 1020,   // Speaking: 17 phút
                _ => 3600
            },
            Difficulty = 3,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Exercises.AddAsync(newExercise);
        await _unitOfWork.SaveChangesAsync();

        return newExercise.Id;
    }
}