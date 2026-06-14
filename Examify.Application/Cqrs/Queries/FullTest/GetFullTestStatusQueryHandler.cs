// Examify.Application/Cqrs/Queries/FullTest/GetFullTestStatusQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Queries.FullTest;

public class GetFullTestStatusQueryHandler : IRequestHandler<GetFullTestStatusQuery, FullTestStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFullTestStatusQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FullTestStatusDto> Handle(GetFullTestStatusQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin Full Test
        var fullTest = await _unitOfWork.Exercises.GetByIdAsync(request.FullTestId);
        if (fullTest == null || !fullTest.IsFullTest)
            throw new NotFoundException($"Full Test with ID {request.FullTestId} not found");

        // 2. Danh sách kỹ năng theo thứ tự (Reading -> Listening -> Writing -> Speaking)
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
            Skills = new List<SkillStatusDto>()
        };

        // 3. Duyệt từng kỹ năng để xác định trạng thái
        for (int i = 0; i < skills.Count; i++)
        {
            var skill = skills[i];
            var status = new SkillStatusDto
            {
                Skill = skill.Skill,
                SkillName = skill.Name,
                ExerciseId = skill.ExerciseId,
                IsUnlocked = true,     // Mặc định kỹ năng đầu tiên được mở
                IsCompleted = false,
                Attempts = 0,
                BestScore = null,
                LatestScore = null,
                LastAttemptAt = null
            };

            // 4. Lấy thông tin bài làm của kỹ năng này (nếu có ExerciseId)
            if (skill.ExerciseId.HasValue)
            {
                var submissions = await _unitOfWork.Submissions
                    .FindAsync(s => s.UserId == request.UserId
                                 && s.ExerciseId == skill.ExerciseId.Value
                                 && s.IsGraded);

                var submissionList = submissions.OrderByDescending(s => s.SubmittedAt).ToList();

                status.Attempts = submissionList.Count;

                if (submissionList.Any())
                {
                    status.IsCompleted = true;
                    status.BestScore = submissionList.Max(s => (double)s.TotalScore);
                    status.LatestScore = submissionList.First().TotalScore;
                    status.LastAttemptAt = submissionList.First().SubmittedAt;
                }
            }

            // 5. Kiểm tra điều kiện mở khóa (chỉ cần kỹ năng trước đã làm)
            if (i > 0)
            {
                var previousSkill = result.Skills[i - 1];

                // Điều kiện: kỹ năng trước đó đã được làm (có attempts > 0)
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

    private class SkillInfo
    {
        public int Skill { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ExerciseId { get; set; }
    }
}