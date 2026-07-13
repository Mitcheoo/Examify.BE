// Examify.Application/Cqrs/Queries/Admin/Submissions/GetAdminSubmissionsQueryHandler.cs
using AutoMapper;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Common;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Admin.Submissions;

public class GetAdminSubmissionsQueryHandler : IRequestHandler<GetAdminSubmissionsQuery, PagedResult<AdminSubmissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAdminSubmissionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<AdminSubmissionDto>> Handle(GetAdminSubmissionsQuery request, CancellationToken cancellationToken)
    {
        // Lấy tất cả submissions
        var allSubmissions = await _unitOfWork.Submissions
            .FindAsync(s => !s.IsDeleted);

        var query = allSubmissions.AsQueryable();

        // 🔍 Filter by search (user name, email, exam title)
        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower().Trim();

            // ✅ SỬA: Bỏ .IsDeleted cho User
            var allUsers = await _unitOfWork.Users.FindAsync(u => true);
            var userIds = allUsers
                .Where(u => u.FullName.ToLower().Contains(search) ||
                           (u.Email != null && u.Email.ToLower().Contains(search)))
                .Select(u => u.Id)
                .ToList();

            // Lấy exerciseIds matching search
            var allExercises = await _unitOfWork.Exercises.FindAsync(e => !e.IsDeleted);
            var exerciseIds = allExercises
                .Where(e => e.Title.ToLower().Contains(search))
                .Select(e => e.Id)
                .ToList();

            query = query.Where(s =>
                userIds.Contains(s.UserId) ||
                exerciseIds.Contains(s.ExerciseId));
        }

        // 🏷️ Filter by skill
        if (!string.IsNullOrEmpty(request.Skill))
        {
            var skillMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Reading", 0 },
                { "Listening", 1 },
                { "Writing", 2 },
                { "Speaking", 3 },
                { "FullTest", 4 }
            };

            if (skillMap.TryGetValue(request.Skill, out var skillValue))
            {
                query = query.Where(s => s.SkillType == skillValue);
            }
        }

        // 📊 Filter by status
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (request.Status.Equals("Passed", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.IsGraded && s.TotalScore >= 7);
            }
            else if (request.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.IsGraded && s.TotalScore < 7);
            }
            else if (request.Status.Equals("InProgress", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => !s.IsGraded);
            }
        }

        // 📅 Filter by date range
        if (request.FromDate.HasValue)
        {
            query = query.Where(s => s.SubmittedAt >= request.FromDate.Value);
        }
        if (request.ToDate.HasValue)
        {
            var endDate = request.ToDate.Value.AddDays(1);
            query = query.Where(s => s.SubmittedAt < endDate);
        }

        // 📌 Sort
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            switch (request.SortBy.ToLower())
            {
                case "username":
                    // ✅ SỬA: Bỏ .IsDeleted cho User
                    var allUsersForSort = await _unitOfWork.Users.FindAsync(u => true);
                    var userDict = allUsersForSort.ToDictionary(u => u.Id, u => u.FullName);

                    query = request.SortDescending
                        ? query.OrderByDescending(s => userDict.GetValueOrDefault(s.UserId, "Unknown"))
                        : query.OrderBy(s => userDict.GetValueOrDefault(s.UserId, "Unknown"));
                    break;
                case "score":
                    query = request.SortDescending
                        ? query.OrderByDescending(s => s.TotalScore)
                        : query.OrderBy(s => s.TotalScore);
                    break;
                case "submittedat":
                default:
                    query = request.SortDescending
                        ? query.OrderByDescending(s => s.SubmittedAt)
                        : query.OrderBy(s => s.SubmittedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(s => s.SubmittedAt);
        }

        // 📄 Pagination
        var totalCount = query.Count();
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var paginatedItems = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // 🗺️ Map to DTOs
        var result = new List<AdminSubmissionDto>();

        // ✅ SỬA: Bỏ .IsDeleted cho User
        var allUsersForDto = await _unitOfWork.Users.FindAsync(u => true);
        var userDictForDto = allUsersForDto.ToDictionary(u => u.Id, u => u);

        var allExercisesForDto = await _unitOfWork.Exercises.FindAsync(e => !e.IsDeleted);
        var exerciseDictForDto = allExercisesForDto.ToDictionary(e => e.Id, e => e);

        foreach (var submission in paginatedItems)
        {
            var user = userDictForDto.GetValueOrDefault(submission.UserId);
            var exercise = exerciseDictForDto.GetValueOrDefault(submission.ExerciseId);

            result.Add(new AdminSubmissionDto
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
            });
        }

        return new PagedResult<AdminSubmissionDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
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