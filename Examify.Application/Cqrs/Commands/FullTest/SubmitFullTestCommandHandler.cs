// Examify.Application/Cqrs/Commands/FullTest/SubmitFullTestCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.FullTest;

public sealed class SubmitFullTestCommandHandler : IRequestHandler<SubmitFullTestCommand, FullTestResultResponse>
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

        // Lấy tất cả bài làm đã chấm điểm của user
        var recentSubmissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == session.UserId && s.IsGraded);

        var submissionsList = recentSubmissions.ToList();

        // Lấy bài làm mới nhất cho từng kỹ năng
        var readingSub = submissionsList
            .Where(s => s.SkillType == 0)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefault();

        var listeningSub = submissionsList
            .Where(s => s.SkillType == 1)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefault();

        var writingSub = submissionsList
            .Where(s => s.SkillType == 2)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefault();

        var speakingSub = submissionsList
            .Where(s => s.SkillType == 3)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefault();

        // Cập nhật SubmissionId
        session.ReadingSubmissionId = readingSub?.Id;
        session.ListeningSubmissionId = listeningSub?.Id;
        session.WritingSubmissionId = writingSub?.Id;
        session.SpeakingSubmissionId = speakingSub?.Id;

        // Cập nhật TimeSpent
        session.ReadingTimeSpent = readingSub?.TimeSpentSeconds ?? 0;
        session.ListeningTimeSpent = listeningSub?.TimeSpentSeconds ?? 0;
        session.WritingTimeSpent = writingSub?.TimeSpentSeconds ?? 0;
        session.SpeakingTimeSpent = speakingSub?.TimeSpentSeconds ?? 0;

        // Tính tổng điểm
        var scores = new double[]
        {
            readingSub?.TotalScore ?? 0,
            listeningSub?.TotalScore ?? 0,
            writingSub?.TotalScore ?? 0,
            speakingSub?.TotalScore ?? 0
        };
        session.TotalScore = (short)Math.Round(scores.Average());
        session.Status = 1;
        session.EndTime = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        // ============================================================
        // ✅ THÊM: XÓA SESSION ANSWERS SAU KHI NỘP BÀI
        // ============================================================

        Console.WriteLine($"🗑️ Deleting draft answers for session: {session.Id}");

        var draftAnswers = await _unitOfWork.SessionAnswers
            .FindAsync(a => a.SessionId == session.Id);

        foreach (var answer in draftAnswers)
        {
            await _unitOfWork.SessionAnswers.DeleteAsync(answer);
            Console.WriteLine($"   ✅ Deleted answer for question: {answer.QuestionId}");
        }

        Console.WriteLine($"✅ All draft answers deleted for session: {session.Id}");

        // Cập nhật Leaderboard
        await UpdateLeaderboard(session.UserId, session.TotalScore.Value);

        // Tính tổng thời gian
        var totalTime = session.ReadingTimeSpent + session.ListeningTimeSpent +
                        session.WritingTimeSpent + session.SpeakingTimeSpent;

        return new FullTestResultResponse
        {
            SessionId = session.Id,
            TotalScore = session.TotalScore ?? 0,
            ReadingScore = readingSub?.TotalScore ?? 0,
            ListeningScore = listeningSub?.TotalScore ?? 0,
            WritingScore = writingSub?.TotalScore ?? 0,
            SpeakingScore = speakingSub?.TotalScore ?? 0,
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