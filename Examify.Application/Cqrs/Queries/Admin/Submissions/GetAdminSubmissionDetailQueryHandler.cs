// Examify.Application/Cqrs/Queries/Admin/Submissions/GetAdminSubmissionDetailQueryHandler.cs
using Examify.Application.DTOs.Admin;
using Examify.Core.Entities;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Admin.Submissions;

public class GetAdminSubmissionDetailQueryHandler : IRequestHandler<GetAdminSubmissionDetailQuery, AdminSubmissionDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminSubmissionDetailQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminSubmissionDto> Handle(GetAdminSubmissionDetailQuery request, CancellationToken cancellationToken)
    {
        // Lấy submission theo ID
        var submission = await _unitOfWork.Submissions.GetByIdAsync(request.Id);

        if (submission == null || submission.IsDeleted)
        {
            throw new NotFoundException($"Không tìm thấy bài nộp với ID '{request.Id}'");
        }

        // Lấy thông tin user
        var user = await _unitOfWork.Users.GetByIdAsync(submission.UserId);

        // Lấy thông tin exercise
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(submission.ExerciseId);

        // Map sang DTO
        return new AdminSubmissionDto
        {
            Id = submission.Id,
            UserId = submission.UserId,
            UserName = user?.FullName ?? "Unknown",
            UserEmail = user?.Email ?? "unknown@email.com",
            ExerciseId = submission.ExerciseId,
            ExerciseTitle = exercise?.Title ?? "Unknown",
            SkillType = submission.SkillType,
            SkillName = GetSkillName(submission.SkillType),
            Score = submission.TotalScore,
            TotalQuestions = submission.TotalQuestions,
            CorrectCount = submission.CorrectCount,
            TimeSpentSeconds = submission.TimeSpentSeconds,
            TimeSpentFormatted = FormatTime(submission.TimeSpentSeconds),
            Status = GetStatus(submission),
            SubmittedAt = submission.SubmittedAt,
            IsGraded = submission.IsGraded,
            AudioUrl = submission.AudioUrl,
            Transcript = submission.Transcript,
            EssayText = submission.EssayText,
            AiFeedback = submission.AiFeedback
        };
    }

    private string GetSkillName(int skillType)
    {
        return skillType switch
        {
            0 => "Reading",
            1 => "Listening",
            2 => "Writing",
            3 => "Speaking",
            4 => "FullTest",
            _ => "Unknown"
        };
    }

    private string GetStatus(Submission submission)
    {
        if (!submission.IsGraded)
            return "In Progress";

        return submission.TotalScore >= 4 ? "Passed" : "Failed";
    }

    private string FormatTime(int seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.Hours > 0)
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m";
        if (timeSpan.Minutes > 0)
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        return $"{timeSpan.Seconds}s";
    }
}