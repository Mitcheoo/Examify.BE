using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Enums;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Queries.Submissions;

public class GetMySubmissionsQueryHandler : IRequestHandler<GetMySubmissionsQuery, List<MySubmissionItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMySubmissionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<MySubmissionItemDto>> Handle(GetMySubmissionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ LẤY TẤT CẢ SUBMISSIONS CỦA USER
        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId && s.IsGraded);

        // ✅ LỌC THEO SKILL NẾU CÓ
        if (request.SkillType.HasValue)
        {
            submissions = submissions.Where(s => s.SkillType == request.SkillType.Value);
        }

        // ✅ SẮP XẾP MỚI NHẤT TRƯỚC
        var submissionList = submissions
            .OrderByDescending(s => s.SubmittedAt)
            .ToList();

        // ✅ PHÂN TRANG
        if (request.Offset.HasValue)
        {
            submissionList = submissionList.Skip(request.Offset.Value).ToList();
        }
        if (request.Limit.HasValue)
        {
            submissionList = submissionList.Take(request.Limit.Value).ToList();
        }

        // ✅ MAP SANG DTO
        var result = new List<MySubmissionItemDto>();
        foreach (var sub in submissionList)
        {
            // Lấy Exercise
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(sub.ExerciseId);

            result.Add(new MySubmissionItemDto
            {
                SubmissionId = sub.Id,
                ExerciseId = sub.ExerciseId,
                ExerciseTitle = exercise?.Title ?? "Unknown Exercise",
                SkillType = sub.SkillType,
                SkillName = ((SkillType)sub.SkillType).ToString(),
                TotalScore = sub.TotalScore,
                TotalQuestions = sub.TotalQuestions,
                CorrectCount = sub.CorrectCount,
                TimeSpentSeconds = sub.TimeSpentSeconds,
                SubmittedAt = sub.SubmittedAt,
                AudioUrl = sub.AudioUrl,
                Transcript = sub.Transcript,
                EssayText = sub.EssayText
            });
        }

        return result;
    }
}