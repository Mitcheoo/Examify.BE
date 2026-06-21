// Examify.Application/Cqrs/Commands/FullTest/StartFullTestCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
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
        var allExercises = await _unitOfWork.Exercises
            .FindAsync(e => !e.IsFullTest && !e.IsDeleted);

        var exercisesList = allExercises.ToList();

        var readingEx = exercisesList.FirstOrDefault(e => e.Skill == 0);
        var listeningEx = exercisesList.FirstOrDefault(e => e.Skill == 1);
        var writingEx = exercisesList.FirstOrDefault(e => e.Skill == 2);
        var speakingEx = exercisesList.FirstOrDefault(e => e.Skill == 3);

        var session = new FullTestSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            StartTime = DateTime.UtcNow,
            Status = 0,
            CurrentPart = 1,
            ReadingExerciseId = readingEx?.Id,
            ListeningExerciseId = listeningEx?.Id,
            WritingExerciseId = writingEx?.Id,
            SpeakingExerciseId = speakingEx?.Id
        };

        await _unitOfWork.FullTestSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        var parts = new List<FullTestPartDto>();

        if (readingEx != null)
        {
            parts.Add(new FullTestPartDto
            {
                PartNumber = 1,
                PartName = "Reading",
                ExerciseId = readingEx.Id,
                TimeLimitSeconds = readingEx.TimeLimitSeconds,
                TotalQuestions = readingEx.TotalQuestions,
                IsCompleted = false
            });
        }

        if (listeningEx != null)
        {
            parts.Add(new FullTestPartDto
            {
                PartNumber = 2,
                PartName = "Listening",
                ExerciseId = listeningEx.Id,
                TimeLimitSeconds = listeningEx.TimeLimitSeconds,
                TotalQuestions = listeningEx.TotalQuestions,
                IsCompleted = false
            });
        }

        if (writingEx != null)
        {
            parts.Add(new FullTestPartDto
            {
                PartNumber = 3,
                PartName = "Writing",
                ExerciseId = writingEx.Id,
                TimeLimitSeconds = writingEx.TimeLimitSeconds,
                TotalQuestions = writingEx.TotalQuestions,
                IsCompleted = false
            });
        }

        if (speakingEx != null)
        {
            parts.Add(new FullTestPartDto
            {
                PartNumber = 4,
                PartName = "Speaking",
                ExerciseId = speakingEx.Id,
                TimeLimitSeconds = speakingEx.TimeLimitSeconds,
                TotalQuestions = speakingEx.TotalQuestions,
                IsCompleted = false
            });
        }

        return new StartFullTestResponse
        {
            SessionId = session.Id,
            StartTime = session.StartTime,
            Parts = parts
        };
    }
}