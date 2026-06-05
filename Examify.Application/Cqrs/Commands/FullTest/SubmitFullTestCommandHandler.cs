using MediatR;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.FullTest;

public class SubmitFullTestCommandHandler : IRequestHandler<SubmitFullTestCommand, FullTestResultResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitFullTestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FullTestResultResponse> Handle(SubmitFullTestCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId);
        if (session is null)
            throw new FullTestException("Session not found");

        short readingScore = 0, listeningScore = 0, writingScore = 0, speakingScore = 0;

        if (session.ReadingSubmissionId.HasValue)
        {
            var reading = await _unitOfWork.Submissions.GetByIdAsync(session.ReadingSubmissionId.Value);
            readingScore = reading?.TotalScore ?? 0;
        }

        if (session.ListeningSubmissionId.HasValue)
        {
            var listening = await _unitOfWork.Submissions.GetByIdAsync(session.ListeningSubmissionId.Value);
            listeningScore = listening?.TotalScore ?? 0;
        }

        if (session.WritingSubmissionId.HasValue)
        {
            var writing = await _unitOfWork.Submissions.GetByIdAsync(session.WritingSubmissionId.Value);
            writingScore = writing?.TotalScore ?? 0;
        }

        if (session.SpeakingSubmissionId.HasValue)
        {
            var speaking = await _unitOfWork.Submissions.GetByIdAsync(session.SpeakingSubmissionId.Value);
            speakingScore = speaking?.TotalScore ?? 0;
        }

        var totalScore = (short)Math.Round((readingScore + listeningScore + writingScore + speakingScore) / 4.0, MidpointRounding.AwayFromZero);

        session.TotalScore = totalScore;
        session.EndTime = DateTime.UtcNow;
        session.Status = 1;

        await _unitOfWork.SaveChangesAsync();

        await UpdateLeaderboard(session.UserId, totalScore);

        var totalTime = session.ReadingTimeSpent + session.ListeningTimeSpent +
                        session.WritingTimeSpent + session.SpeakingTimeSpent;

        return new FullTestResultResponse
        {
            SessionId = session.Id,
            TotalScore = totalScore,
            ReadingScore = readingScore,
            ListeningScore = listeningScore,
            WritingScore = writingScore,
            SpeakingScore = speakingScore,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            TotalTimeSpentSeconds = totalTime
        };
    }

    private async Task UpdateLeaderboard(Guid userId, short totalScore)
    {
        var leaderboard = await _unitOfWork.Leaderboards
            .FindAsync(l => l.UserId == userId && l.SkillType == 4);

        var entry = leaderboard.FirstOrDefault();
        if (entry is null)
        {
            entry = new Leaderboard
            {
                UserId = userId,
                SkillType = 4,
                TotalScore = totalScore,
                TotalAttempts = 1,
                AverageScore = totalScore,
                LastUpdated = DateTime.UtcNow
            };
            await _unitOfWork.Leaderboards.AddAsync(entry);
        }
        else
        {
            entry.TotalScore = totalScore;
            entry.TotalAttempts++;
            entry.AverageScore = (entry.AverageScore * (entry.TotalAttempts - 1) + totalScore) / entry.TotalAttempts;
            entry.LastUpdated = DateTime.UtcNow;
            await _unitOfWork.Leaderboards.UpdateAsync(entry);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}