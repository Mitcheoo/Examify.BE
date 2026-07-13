// Examify.Application/Cqrs/Queries/Exercises/GetExerciseStatsQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetExerciseStatsQueryHandler : IRequestHandler<GetExerciseStatsQuery, List<ExerciseStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExerciseStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ExerciseStatsDto>> Handle(GetExerciseStatsQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy tất cả submissions của user
        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId && !s.IsDeleted && s.IsGraded);

        // 2. Lọc theo skill nếu có
        if (request.Skill.HasValue)
        {
            submissions = submissions.Where(s => s.SkillType == request.Skill.Value).ToList();
        }

        // 3. Không có submission nào
        if (!submissions.Any())
        {
            return new List<ExerciseStatsDto>();
        }

        // ============================================================
        // ✅ CÁCH 1: LẤY TẤT CẢ EXERCISE TRƯỚC (Khuyến nghị)
        // ============================================================

        // Lấy tất cả ExerciseId cần thiết
        var exerciseIds = submissions.Select(s => s.ExerciseId).Distinct().ToList();

        // ✅ CHỈ GỌI DB 1 LẦN DUY NHẤT
        var exercises = await _unitOfWork.Exercises
            .FindAsync(e => exerciseIds.Contains(e.Id));

        var exerciseDict = exercises.ToDictionary(e => e.Id);

        // 4. Nhóm theo ExerciseId và tính toán (KHÔNG async)
        var stats = submissions
            .GroupBy(s => s.ExerciseId)
            .Select(g =>
            {
                var exercise = exerciseDict.GetValueOrDefault(g.Key);
                var exerciseSubmissions = g.ToList();

                return new ExerciseStatsDto
                {
                    ExerciseId = g.Key,
                    ExerciseTitle = exercise?.Title ?? "Unknown",
                    Skill = exercise?.Skill ?? 0,
                    SkillName = GetSkillName(exercise?.Skill ?? 0),
                    AttemptCount = exerciseSubmissions.Count,
                    BestScore = exerciseSubmissions.Max(s => s.TotalScore),
                    AverageScore = Math.Round(exerciseSubmissions.Average(s => s.TotalScore), 1),
                    LastScore = exerciseSubmissions.OrderByDescending(s => s.SubmittedAt).First().TotalScore,
                    LastSubmittedAt = exerciseSubmissions.Max(s => s.SubmittedAt)
                };
            })
            .ToList();

        return stats.OrderByDescending(s => s.LastSubmittedAt).ToList();
    }

    private static string GetSkillName(int skill)
    {
        return skill switch
        {
            0 => "Reading",
            1 => "Listening",
            2 => "Writing",
            3 => "Speaking",
            4 => "Full Test",
            _ => "Unknown"
        };
    }
}



//loi 2 luong 
/*// Examify.Application/Cqrs/Queries/Exercises/GetExerciseStatsQueryHandler.cs
using MediatR;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Exercises;

namespace Examify.Application.Cqrs.Queries.Exercises;

public class GetExerciseStatsQueryHandler : IRequestHandler<GetExerciseStatsQuery, List<ExerciseStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExerciseStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ExerciseStatsDto>> Handle(GetExerciseStatsQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy tất cả submissions của user
        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId && !s.IsDeleted && s.IsGraded);

        // 2. Lọc theo skill nếu có
        if (request.Skill.HasValue)
        {
            submissions = submissions.Where(s => s.SkillType == request.Skill.Value).ToList();
        }

        // 3. Không có submission nào
        if (!submissions.Any())
        {
            return new List<ExerciseStatsDto>();
        }

        // 4. Nhóm theo ExerciseId và tính toán
        var stats = submissions
            .GroupBy(s => s.ExerciseId)
            .Select(async g =>
            {
                var exercise = await _unitOfWork.Exercises.GetByIdAsync(g.Key);
                var exerciseSubmissions = g.ToList();

                return new ExerciseStatsDto
                {
                    ExerciseId = g.Key,
                    ExerciseTitle = exercise?.Title ?? "Unknown",
                    Skill = exercise?.Skill ?? 0,
                    SkillName = GetSkillName(exercise?.Skill ?? 0),
                    AttemptCount = exerciseSubmissions.Count,
                    BestScore = exerciseSubmissions.Max(s => s.TotalScore),
                    AverageScore = Math.Round(exerciseSubmissions.Average(s => s.TotalScore), 1),
                    LastScore = exerciseSubmissions.OrderByDescending(s => s.SubmittedAt).First().TotalScore,
                    LastSubmittedAt = exerciseSubmissions.Max(s => s.SubmittedAt)
                };
            });

        var result = await Task.WhenAll(stats);
        return result.OrderByDescending(s => s.LastSubmittedAt).ToList();
    }

    private static string GetSkillName(int skill)
    {
        return skill switch
        {
            0 => "Reading",
            1 => "Listening",
            2 => "Writing",
            3 => "Speaking",
            4 => "Full Test",
            _ => "Unknown"
        };
    }
}*/