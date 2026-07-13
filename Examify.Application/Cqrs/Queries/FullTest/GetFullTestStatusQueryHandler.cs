// 📁 Examify.Application/Cqrs/Queries/FullTest/GetFullTestStatusQueryHandler.cs

using MediatR;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Queries.FullTest;

public sealed class GetFullTestStatusQueryHandler : IRequestHandler<GetFullTestStatusQuery, FullTestStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFullTestStatusQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FullTestStatusDto> Handle(GetFullTestStatusQuery request, CancellationToken cancellationToken)
    {
        var fullTest = await _unitOfWork.Exercises.GetByIdAsync(request.FullTestId);
        if (fullTest is null || !fullTest.IsFullTest)
            throw new NotFoundException($"Full Test with ID {request.FullTestId} not found");

        // ✅ LẤY TẤT CẢ SUBMISSIONS CỦA USER
        var allSubmissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId && s.IsGraded);

        // ✅ LẤY SESSION HIỆN TẠI (IN PROGRESS)
        var currentSessions = await _unitOfWork.FullTestSessions
            .FindAsync(s => s.UserId == request.UserId && s.FullTestId == request.FullTestId && s.Status == 0);

        var currentSession = currentSessions.FirstOrDefault();

        // ✅ LẤY SUBMISSION IDs TỪ SESSION HIỆN TẠI
        var sessionSubmissionIds = new HashSet<Guid>();
        if (currentSession != null)
        {
            if (currentSession.ReadingSubmissionId.HasValue)
                sessionSubmissionIds.Add(currentSession.ReadingSubmissionId.Value);
            if (currentSession.ListeningSubmissionId.HasValue)
                sessionSubmissionIds.Add(currentSession.ListeningSubmissionId.Value);
            if (currentSession.WritingSubmissionId.HasValue)
                sessionSubmissionIds.Add(currentSession.WritingSubmissionId.Value);
            if (currentSession.SpeakingSubmissionId.HasValue)
                sessionSubmissionIds.Add(currentSession.SpeakingSubmissionId.Value);
        }

        var skills = new List<SkillInfo>
        {
            new() { Skill = 0, Name = "Reading", ExerciseId = fullTest.ReadingExerciseId },
            new() { Skill = 1, Name = "Listening", ExerciseId = fullTest.ListeningExerciseId },
            new() { Skill = 2, Name = "Writing", ExerciseId = fullTest.WritingExerciseId },
            new() { Skill = 3, Name = "Speaking", ExerciseId = fullTest.SpeakingExerciseId }
        };

        var result = new FullTestStatusDto
        {
            FullTestId = fullTest.Id,
            FullTestTitle = fullTest.Title,
            Skills = []
        };

        for (int i = 0; i < skills.Count; i++)
        {
            var skill = skills[i];

            // ✅ LỌC SUBMISSIONS THEO SKILL
            var skillSubmissions = allSubmissions
                .Where(s => s.ExerciseId == skill.ExerciseId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToList();

            var status = new SkillStatusDto
            {
                Skill = skill.Skill,
                SkillName = skill.Name,
                ExerciseId = skill.ExerciseId,
                IsUnlocked = true,
                IsCompleted = false,
                Attempts = 0,
                BestScore = null,
                LatestScore = null,
                LastAttemptAt = null
            };

            // ✅ KIỂM TRA SUBMISSIONS CỦA SKILL NÀY
            if (skill.ExerciseId.HasValue && skillSubmissions.Any())
            {
                status.Attempts = skillSubmissions.Count;

                // ✅ CHỈ COMPLETED KHI SUBMISSION THUỘC SESSION HIỆN TẠI
                var hasValidSubmission = skillSubmissions.Any(s => sessionSubmissionIds.Contains(s.Id));

                if (hasValidSubmission)
                {
                    status.IsCompleted = true;
                    status.BestScore = skillSubmissions.Max(s => (double)s.TotalScore);
                    status.LatestScore = skillSubmissions.First().TotalScore;
                    status.LastAttemptAt = skillSubmissions.First().SubmittedAt;
                }
                else
                {
                    status.IsCompleted = false;
                    status.BestScore = null;
                    status.LatestScore = null;
                    status.LastAttemptAt = null;
                }
            }

            // ✅ LOGIC UNLOCK: Skill sau mở khóa khi skill trước đã hoàn thành trong session hiện tại
            if (i > 0)
            {
                var previousSkill = result.Skills[i - 1];

                if (previousSkill.IsCompleted)
                {
                    status.IsUnlocked = true;
                    status.Message = $"Đã hoàn thành {previousSkill.SkillName}, được mở khóa";
                }
                else
                {
                    status.IsUnlocked = false;
                    status.RequiredSkill = previousSkill.Skill;
                    status.RequiredSkillName = previousSkill.SkillName;
                    status.Message = $"Cần hoàn thành {previousSkill.SkillName} trước để mở khóa";
                }
            }

            result.Skills.Add(status);
        }

        return result;
    }

    private sealed class SkillInfo
    {
        public int Skill { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ExerciseId { get; set; }
    }
}