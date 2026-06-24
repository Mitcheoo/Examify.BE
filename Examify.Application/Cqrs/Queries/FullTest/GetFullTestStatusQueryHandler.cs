// Examify.Application/Cqrs/Queries/FullTest/GetFullTestStatusQueryHandler.cs
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

            if (skill.ExerciseId.HasValue)
            {
                var submissions = await _unitOfWork.Submissions
                    .FindAsync(s => s.UserId == request.UserId
                                 && s.ExerciseId == skill.ExerciseId.Value
                                 && s.IsGraded);

                var submissionList = submissions.OrderByDescending(s => s.SubmittedAt).ToList();
                status.Attempts = submissionList.Count;

                if (submissionList.Count != 0)
                {
                    status.IsCompleted = true;
                    status.BestScore = submissionList.Max(s => (double)s.TotalScore);
                    status.LatestScore = submissionList[0].TotalScore;
                    status.LastAttemptAt = submissionList[0].SubmittedAt;
                }
            }

            if (i > 0)
            {
                var previousSkill = result.Skills[i - 1];

                if (previousSkill.Attempts > 0)
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