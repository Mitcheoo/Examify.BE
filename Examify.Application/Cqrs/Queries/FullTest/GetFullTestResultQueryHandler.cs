// Examify.Application/Cqrs/Queries/FullTest/GetFullTestResultQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Queries.FullTest;

public sealed class GetFullTestResultQueryHandler : IRequestHandler<GetFullTestResultQuery, FullTestResultDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFullTestResultQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FullTestResultDetailDto> Handle(GetFullTestResultQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin Full Test
        var fullTest = await _unitOfWork.Exercises.GetByIdAsync(request.FullTestId);
        if (fullTest is null || !fullTest.IsFullTest)
            throw new NotFoundException("Full Test not found");

        // 2. Lấy session đã hoàn thành
        var sessions = await _unitOfWork.FullTestSessions
            .FindAsync(s => s.UserId == request.UserId && s.Status == 1);

        var session = sessions.OrderByDescending(s => s.EndTime).FirstOrDefault();
        if (session is null)
            throw new NotFoundException("No completed session found for this Full Test");

        // 3. Khởi tạo dữ liệu
        var skillResults = new List<SkillResultDto>();
        int totalQuestions = 0;
        int correctCount = 0;
        DateTime submittedAt = DateTime.UtcNow;

        // ============================================================
        // 4. XỬ LÝ TỪNG KỸ NĂNG
        // ============================================================

        // === READING ===
        if (session.ReadingSubmissionId.HasValue)
        {
            var readingSubmission = await _unitOfWork.Submissions.GetByIdAsync(session.ReadingSubmissionId.Value);
            if (readingSubmission != null)
            {
                totalQuestions += readingSubmission.TotalQuestions;
                correctCount += readingSubmission.CorrectCount;
                if (readingSubmission.SubmittedAt > submittedAt)
                    submittedAt = readingSubmission.SubmittedAt;
            }

            skillResults.Add(new SkillResultDto
            {
                Skill = 0,
                SkillName = "Reading",
                SubmissionId = session.ReadingSubmissionId,
                Score = readingSubmission?.TotalScore ?? 0,
                TimeSpentSeconds = session.ReadingTimeSpent,
                SubmittedAt = readingSubmission?.SubmittedAt ?? session.EndTime ?? DateTime.UtcNow,
                IsCompleted = readingSubmission is not null,
                TotalQuestions = readingSubmission?.TotalQuestions ?? 0,
                CorrectCount = readingSubmission?.CorrectCount ?? 0,
                Status = readingSubmission is not null ? "completed" : "pending"
            });
        }

        // === LISTENING ===
        if (session.ListeningSubmissionId.HasValue)
        {
            var listeningSubmission = await _unitOfWork.Submissions.GetByIdAsync(session.ListeningSubmissionId.Value);
            if (listeningSubmission != null)
            {
                totalQuestions += listeningSubmission.TotalQuestions;
                correctCount += listeningSubmission.CorrectCount;
                if (listeningSubmission.SubmittedAt > submittedAt)
                    submittedAt = listeningSubmission.SubmittedAt;
            }

            skillResults.Add(new SkillResultDto
            {
                Skill = 1,
                SkillName = "Listening",
                SubmissionId = session.ListeningSubmissionId,
                Score = listeningSubmission?.TotalScore ?? 0,
                TimeSpentSeconds = session.ListeningTimeSpent,
                SubmittedAt = listeningSubmission?.SubmittedAt ?? session.EndTime ?? DateTime.UtcNow,
                IsCompleted = listeningSubmission is not null,
                TotalQuestions = listeningSubmission?.TotalQuestions ?? 0,
                CorrectCount = listeningSubmission?.CorrectCount ?? 0,
                Status = listeningSubmission is not null ? "completed" : "pending"
            });
        }

        // === WRITING ===
        if (session.WritingSubmissionId.HasValue)
        {
            var writingSubmission = await _unitOfWork.Submissions.GetByIdAsync(session.WritingSubmissionId.Value);
            if (writingSubmission != null)
            {
                totalQuestions += writingSubmission.TotalQuestions;
                correctCount += writingSubmission.CorrectCount;
                if (writingSubmission.SubmittedAt > submittedAt)
                    submittedAt = writingSubmission.SubmittedAt;
            }

            skillResults.Add(new SkillResultDto
            {
                Skill = 2,
                SkillName = "Writing",
                SubmissionId = session.WritingSubmissionId,
                Score = writingSubmission?.TotalScore ?? 0,
                TimeSpentSeconds = session.WritingTimeSpent,
                SubmittedAt = writingSubmission?.SubmittedAt ?? session.EndTime ?? DateTime.UtcNow,
                IsCompleted = writingSubmission is not null,
                TotalQuestions = writingSubmission?.TotalQuestions ?? 0,
                CorrectCount = writingSubmission?.CorrectCount ?? 0,
                Status = writingSubmission is not null ? "completed" : "pending"
            });
        }

        // === SPEAKING ===
        if (session.SpeakingSubmissionId.HasValue)
        {
            var speakingSubmission = await _unitOfWork.Submissions.GetByIdAsync(session.SpeakingSubmissionId.Value);
            if (speakingSubmission != null)
            {
                totalQuestions += speakingSubmission.TotalQuestions;
                correctCount += speakingSubmission.CorrectCount;
                if (speakingSubmission.SubmittedAt > submittedAt)
                    submittedAt = speakingSubmission.SubmittedAt;
            }

            skillResults.Add(new SkillResultDto
            {
                Skill = 3,
                SkillName = "Speaking",
                SubmissionId = session.SpeakingSubmissionId,
                Score = speakingSubmission?.TotalScore ?? 0,
                TimeSpentSeconds = session.SpeakingTimeSpent,
                SubmittedAt = speakingSubmission?.SubmittedAt ?? session.EndTime ?? DateTime.UtcNow,
                IsCompleted = speakingSubmission is not null,
                TotalQuestions = speakingSubmission?.TotalQuestions ?? 0,
                CorrectCount = speakingSubmission?.CorrectCount ?? 0,
                Status = speakingSubmission is not null ? "completed" : "pending"
            });
        }

        // 5. Tính tổng thời gian
        var totalTimeSpent = session.ReadingTimeSpent + session.ListeningTimeSpent +
                             session.WritingTimeSpent + session.SpeakingTimeSpent;

        // 6. Trả về kết quả
        return new FullTestResultDetailDto
        {
            SessionId = session.Id,
            FullTestId = fullTest.Id,
            FullTestTitle = fullTest.Title,
            StartedAt = session.StartTime,
            CompletedAt = session.EndTime,
            TotalTimeSpentSeconds = totalTimeSpent,
            TotalScore = session.TotalScore ?? 0,
            TotalQuestions = totalQuestions,
            CorrectCount = correctCount,
            SubmittedAt = submittedAt,
            SkillResults = skillResults.OrderBy(s => s.Skill).ToList()
        };
    }
}