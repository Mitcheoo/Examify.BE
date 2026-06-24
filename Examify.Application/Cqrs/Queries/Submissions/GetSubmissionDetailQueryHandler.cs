/*// Examify.Application/Cqrs/Queries/Submissions/GetSubmissionDetailQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Submissions;
using System.Text.Json;

namespace Examify.Application.Cqrs.Queries.Submissions;

public class GetSubmissionDetailQueryHandler
    : IRequestHandler<GetSubmissionDetailQuery, SubmissionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSubmissionDetailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)  
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SubmissionDetailDto> Handle(
        GetSubmissionDetailQuery request,
        CancellationToken cancellationToken)
    {
        var submission = await _unitOfWork.Submissions.GetByIdAsync(request.Id);    
        if (submission == null)
            throw new NotFoundException($"Submission with ID {request.Id} not found");

        if (submission.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not authorized to view this submission");

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(submission.ExerciseId);
        if (exercise == null)
            throw new NotFoundException($"Exercise with ID {submission.ExerciseId} not found");

        var result = new SubmissionDetailDto
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
            Details = new List<SubmissionAnswerDetailDto>(),
            AiFeedback = null
        };

        // Lấy chi tiết câu hỏi từ AnswerJson
        if (!string.IsNullOrEmpty(submission.AnswerJson))
        {
            try
            {
                var answers = JsonSerializer.Deserialize<List<AnswerDto>>(submission.AnswerJson);
                if (answers != null)
                {
                    foreach (var answer in answers)
                    {
                        // Lấy thông tin câu hỏi tương ứng
                        var questionText = await GetQuestionText(submission.ExerciseId, answer.QuestionId);
                        var correctAnswer = await GetCorrectAnswer(submission.ExerciseId, answer.QuestionId);

                        result.Details.Add(new SubmissionAnswerDetailDto
                        {
                            QuestionId = answer.QuestionId,
                            QuestionText = questionText,
                            UserAnswer = answer.SelectedOption ?? answer.EssayText ?? "N/A",
                            CorrectAnswer = correctAnswer,
                            IsCorrect = false // Sẽ tính sau
                        });
                    }
                }
            }
            catch
            {
                // Bỏ qua nếu không parse được
            }
        }

        return result;
    }

    private string GetSkillName(int skill)
    {
        return skill switch
        {
            0 => "Reading",
            1 => "Listening",
            2 => "Writing",
            3 => "Speaking",
            _ => "Unknown"
        };
    }

    private async Task<string> GetQuestionText(Guid exerciseId, Guid questionId)
    {
        // Thử tìm trong ReadingQuestions
        var readingQ = await _unitOfWork.ReadingQuestions.GetByIdAsync(questionId);
        if (readingQ != null) return readingQ.QuestionText;

        // Thử tìm trong ListeningQuestions
        var listeningQ = await _unitOfWork.ListeningQuestions.GetByIdAsync(questionId);
        if (listeningQ != null) return listeningQ.QuestionText;

        // Thử tìm trong WritingQuestions
        var writingQ = await _unitOfWork.WritingQuestions.GetByIdAsync(questionId);
        if (writingQ != null) return writingQ.PromptText;

        // Thử tìm trong SpeakingQuestions
        var speakingQ = await _unitOfWork.SpeakingQuestions.GetByIdAsync(questionId);
        if (speakingQ != null) return speakingQ.QuestionText;

        return "Unknown Question";
    }

    private async Task<string?> GetCorrectAnswer(Guid exerciseId, Guid questionId)
    {
        var readingQ = await _unitOfWork.ReadingQuestions.GetByIdAsync(questionId);
        if (readingQ != null) return readingQ.CorrectAnswer;

        var listeningQ = await _unitOfWork.ListeningQuestions.GetByIdAsync(questionId);
        if (listeningQ != null) return listeningQ.CorrectAnswer;

        return null;
    }
}*/