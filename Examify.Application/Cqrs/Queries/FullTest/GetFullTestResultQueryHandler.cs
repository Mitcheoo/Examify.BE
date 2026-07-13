// 📁 Examify.Application/Cqrs/Queries/FullTest/GetFullTestResultQueryHandler.cs

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

        // 2. Lấy session đã hoàn thành CÓ SUBMISSION
        var sessions = await _unitOfWork.FullTestSessions
            .FindAsync(s => s.UserId == request.UserId && s.Status == 1);

        var validSession = sessions
            .Where(s => s.ReadingSubmissionId.HasValue
                     || s.ListeningSubmissionId.HasValue
                     || s.WritingSubmissionId.HasValue
                     || s.SpeakingSubmissionId.HasValue)
            .OrderByDescending(s => s.EndTime)
            .FirstOrDefault();

        if (validSession is null)
        {
            return new FullTestResultDetailDto
            {
                FullTestId = fullTest.Id,
                FullTestTitle = fullTest.Title,
                TotalScore = 0,
                TotalQuestions = 0,
                CorrectCount = 0,
                TotalTimeSpentSeconds = 0,
                SkillResults = new List<SkillResultDto>()
            };
        }

        // 3. Khởi tạo dữ liệu
        var skillResults = new List<SkillResultDto>();
        int totalQuestions = 0;
        int correctCount = 0;
        DateTime submittedAt = DateTime.UtcNow;

        // ============================================================
        // 4. XỬ LÝ TỪNG KỸ NĂNG
        // ============================================================

        // === READING ===
        if (validSession.ReadingSubmissionId.HasValue)
        {
            var readingSubmission = await _unitOfWork.Submissions.GetByIdAsync(validSession.ReadingSubmissionId.Value);
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
                SubmissionId = validSession.ReadingSubmissionId,
                Score = readingSubmission?.TotalScore ?? 0,
                TimeSpentSeconds = validSession.ReadingTimeSpent,
                SubmittedAt = readingSubmission?.SubmittedAt ?? validSession.EndTime ?? DateTime.UtcNow,
                IsCompleted = readingSubmission is not null,
                TotalQuestions = readingSubmission?.TotalQuestions ?? 0,
                CorrectCount = readingSubmission?.CorrectCount ?? 0,
                Status = readingSubmission is not null ? "completed" : "pending",
                AiFeedback = readingSubmission?.AiFeedback  // ✅ THÊM DÒNG NÀY
            });
        }
        else
        {
            skillResults.Add(new SkillResultDto
            {
                Skill = 0,
                SkillName = "Reading",
                SubmissionId = null,
                Score = 0,
                TimeSpentSeconds = 0,
                SubmittedAt = null,
                IsCompleted = false,
                TotalQuestions = 0,
                CorrectCount = 0,
                Status = "pending",
                AiFeedback = null  // ✅ THÊM DÒNG NÀY
            });
        }

        // === LISTENING ===
        if (validSession.ListeningSubmissionId.HasValue)
        {
            var listeningSubmission = await _unitOfWork.Submissions.GetByIdAsync(validSession.ListeningSubmissionId.Value);
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
                SubmissionId = validSession.ListeningSubmissionId,
                Score = listeningSubmission?.TotalScore ?? 0,
                TimeSpentSeconds = validSession.ListeningTimeSpent,
                SubmittedAt = listeningSubmission?.SubmittedAt ?? validSession.EndTime ?? DateTime.UtcNow,
                IsCompleted = listeningSubmission is not null,
                TotalQuestions = listeningSubmission?.TotalQuestions ?? 0,
                CorrectCount = listeningSubmission?.CorrectCount ?? 0,
                Status = listeningSubmission is not null ? "completed" : "pending",
                AiFeedback = listeningSubmission?.AiFeedback  // ✅ THÊM DÒNG NÀY
            });
        }
        else
        {
            skillResults.Add(new SkillResultDto
            {
                Skill = 1,
                SkillName = "Listening",
                SubmissionId = null,
                Score = 0,
                TimeSpentSeconds = 0,
                SubmittedAt = null,
                IsCompleted = false,
                TotalQuestions = 0,
                CorrectCount = 0,
                Status = "pending",
                AiFeedback = null  // ✅ THÊM DÒNG NÀY
            });
        }

        // === WRITING ===
        if (validSession.WritingSubmissionId.HasValue)
        {
            var writingSubmission = await _unitOfWork.Submissions.GetByIdAsync(validSession.WritingSubmissionId.Value);
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
                SubmissionId = validSession.WritingSubmissionId,
                Score = writingSubmission?.TotalScore ?? 0,
                TimeSpentSeconds = validSession.WritingTimeSpent,
                SubmittedAt = writingSubmission?.SubmittedAt ?? validSession.EndTime ?? DateTime.UtcNow,
                IsCompleted = writingSubmission is not null,
                TotalQuestions = writingSubmission?.TotalQuestions ?? 0,
                CorrectCount = writingSubmission?.CorrectCount ?? 0,
                Status = writingSubmission is not null ? "completed" : "pending",
                AiFeedback = writingSubmission?.AiFeedback  // ✅ THÊM DÒNG NÀY
            });
        }
        else
        {
            skillResults.Add(new SkillResultDto
            {
                Skill = 2,
                SkillName = "Writing",
                SubmissionId = null,
                Score = 0,
                TimeSpentSeconds = 0,
                SubmittedAt = null,
                IsCompleted = false,
                TotalQuestions = 0,
                CorrectCount = 0,
                Status = "pending",
                AiFeedback = null  // ✅ THÊM DÒNG NÀY
            });
        }

        // === SPEAKING ===
        if (validSession.SpeakingSubmissionId.HasValue)
        {
            var speakingSubmission = await _unitOfWork.Submissions.GetByIdAsync(validSession.SpeakingSubmissionId.Value);
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
                SubmissionId = validSession.SpeakingSubmissionId,
                Score = speakingSubmission?.TotalScore ?? 0,
                TimeSpentSeconds = validSession.SpeakingTimeSpent,
                SubmittedAt = speakingSubmission?.SubmittedAt ?? validSession.EndTime ?? DateTime.UtcNow,
                IsCompleted = speakingSubmission is not null,
                TotalQuestions = speakingSubmission?.TotalQuestions ?? 0,
                CorrectCount = speakingSubmission?.CorrectCount ?? 0,
                Status = speakingSubmission is not null ? "completed" : "pending",
                AiFeedback = speakingSubmission?.AiFeedback  // ✅ THÊM DÒNG NÀY
            });
        }
        else
        {
            skillResults.Add(new SkillResultDto
            {
                Skill = 3,
                SkillName = "Speaking",
                SubmissionId = null,
                Score = 0,
                TimeSpentSeconds = 0,
                SubmittedAt = null,
                IsCompleted = false,
                TotalQuestions = 0,
                CorrectCount = 0,
                Status = "pending",
                AiFeedback = null  // ✅ THÊM DÒNG NÀY
            });
        }

        // 5. Tính tổng thời gian
        var totalTimeSpent = validSession.ReadingTimeSpent + validSession.ListeningTimeSpent +
                             validSession.WritingTimeSpent + validSession.SpeakingTimeSpent;

        // 6. Trả về kết quả
        return new FullTestResultDetailDto
        {
            SessionId = validSession.Id,
            FullTestId = fullTest.Id,
            FullTestTitle = fullTest.Title,
            StartedAt = validSession.StartTime,
            CompletedAt = validSession.EndTime,
            TotalTimeSpentSeconds = totalTimeSpent,
            TotalScore = validSession.TotalScore ?? 0,
            TotalQuestions = totalQuestions,
            CorrectCount = correctCount,
            SubmittedAt = submittedAt,
            SkillResults = skillResults.OrderBy(s => s.Skill).ToList()
        };
    }
}