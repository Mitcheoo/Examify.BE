// Examify.Application/Cqrs/Queries/Submissions/GetSubmissionResultQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Submissions;
using System.Text.Json;

namespace Examify.Application.Cqrs.Queries.Submissions;

public class GetSubmissionResultQueryHandler : IRequestHandler<GetSubmissionResultQuery, SubmissionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSubmissionResultQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SubmissionDetailDto> Handle(GetSubmissionResultQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy Submission
        var submission = await _unitOfWork.Submissions.GetByIdAsync(request.SubmissionId);
        if (submission == null)
            throw new NotFoundException("Submission not found");

        // 2. Lấy Exercise
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(submission.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        // 3. Lấy danh sách SubmissionDetail
        var details = await _unitOfWork.SubmissionDetails
            .FindAsync(d => d.SubmissionId == request.SubmissionId);

        var detailList = details.OrderBy(d => d.OrderNumber).ToList();

        // 4. Map sang SubmissionAnswerDetailDto
        var answerDetails = new List<SubmissionAnswerDetailDto>();

        foreach (var detail in detailList)
        {
            string questionText = await GetQuestionText(submission.ExerciseId, detail.QuestionId, submission.SkillType);
            string? explanation = await GetExplanation(submission.ExerciseId, detail.QuestionId, submission.SkillType);

            answerDetails.Add(new SubmissionAnswerDetailDto
            {
                QuestionId = detail.QuestionId,
                QuestionText = questionText,
                OrderNumber = detail.OrderNumber,
                UserAnswer = detail.UserAnswer,
                CorrectAnswer = detail.CorrectAnswer,
                IsCorrect = detail.IsCorrect,
                AiScore = detail.AiScore,
                AiFeedback = detail.AiFeedback,
                Explanation = explanation ?? detail.Explanation
            });
        }

        // 5. Xử lý AiFeedback
        object? aiFeedback = null;
        if (!string.IsNullOrEmpty(submission.AiFeedback))
        {
            try
            {
                aiFeedback = JsonSerializer.Deserialize<object>(submission.AiFeedback);
            }
            catch
            {
                aiFeedback = submission.AiFeedback;
            }
        }

        // 6. Trả về kết quả - ✅ THÊM 3 TRƯỜNG
        return new SubmissionDetailDto
        {
            Id = submission.Id,
            ExerciseId = submission.ExerciseId,
            ExerciseTitle = exercise.Title,
            Skill = submission.SkillType,
            SkillName = GetSkillName(submission.SkillType),
            TotalScore = submission.TotalScore,
            TotalQuestions = submission.TotalQuestions,
            CorrectCount = submission.CorrectCount,
            SubmittedAt = submission.SubmittedAt,

            // ✅ THÊM 3 DÒNG NÀY
            AudioUrl = submission.AudioUrl,
            Transcript = submission.Transcript,
            AnswerJson = submission.AnswerJson,

            Details = answerDetails,
            AiFeedback = aiFeedback
        };
    }

    /// <summary>
    /// Lấy nội dung câu hỏi dựa trên SkillType
    /// </summary>
    private async Task<string> GetQuestionText(Guid exerciseId, Guid questionId, int skillType)
    {
        try
        {
            switch (skillType)
            {
                case 0: // Reading
                    var readingQ = await _unitOfWork.ReadingQuestions.GetByIdAsync(questionId);
                    return readingQ?.QuestionText ?? "Unknown Question";
                case 1: // Listening
                    var listeningQ = await _unitOfWork.ListeningQuestions.GetByIdAsync(questionId);
                    return listeningQ?.QuestionText ?? "Unknown Question";
                case 2: // Writing
                    var writingQ = await _unitOfWork.WritingQuestions.GetByIdAsync(questionId);
                    return writingQ?.PromptText ?? "Unknown Question";
                case 3: // Speaking
                    var speakingQ = await _unitOfWork.SpeakingQuestions.GetByIdAsync(questionId);
                    return speakingQ?.QuestionText ?? "Unknown Question";
                default:
                    return "Unknown Question";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error getting question text: {ex.Message}");
            return "Unknown Question";
        }
    }

    /// <summary>
    /// Lấy giải thích câu hỏi dựa trên SkillType
    /// </summary>
    private async Task<string?> GetExplanation(Guid exerciseId, Guid questionId, int skillType)
    {
        try
        {
            switch (skillType)
            {
                case 0: // Reading
                    var readingQ = await _unitOfWork.ReadingQuestions.GetByIdAsync(questionId);
                    return readingQ?.Explanation;
                case 1: // Listening
                    var listeningQ = await _unitOfWork.ListeningQuestions.GetByIdAsync(questionId);
                    return listeningQ?.Explanation;
                case 2: // Writing
                    return null;
                case 3: // Speaking
                    return null;
                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Lấy tên kỹ năng từ SkillType
    /// </summary>
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