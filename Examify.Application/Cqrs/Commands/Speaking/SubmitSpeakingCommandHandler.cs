// Examify.Application/Cqrs/Commands/Speaking/SubmitSpeakingCommandHandler.cs
using MediatR;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Core.Exceptions;
using Examify.Application.DTOs.Submissions;
using System.Text.Json;

namespace Examify.Application.Cqrs.Commands.Speaking;

public sealed class SubmitSpeakingCommandHandler : IRequestHandler<SubmitSpeakingCommand, SubmissionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIGradingService _aiGradingService;
    private readonly IFileStorageService _fileStorageService;

    public SubmitSpeakingCommandHandler(
        IUnitOfWork unitOfWork,
        IAIGradingService aiGradingService,
        IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _aiGradingService = aiGradingService;
        _fileStorageService = fileStorageService;
    }

    public async Task<SubmissionDetailDto> Handle(SubmitSpeakingCommand request, CancellationToken cancellationToken)
    {
        // ============================================================
        // 1. LẤY CÂU HỎI SPEAKING
        // ============================================================
        var questions = await _unitOfWork.SpeakingQuestions
            .FindAsync(q => q.ExerciseId == request.ExerciseId && !q.IsDeleted);

        var questionList = questions.OrderBy(q => q.OrderNumber).ToList();
        if (questionList.Count == 0)
            throw new NotFoundException("Speaking questions not found");

        // ============================================================
        // 2. LẤY THÔNG TIN EXERCISE
        // ============================================================
        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        // ============================================================
        // 3. KHỞI TẠO DỮ LIỆU
        // ============================================================
        List<string> audioUrls = new();
        Dictionary<Guid, string> transcripts = new();

        // ============================================================
        // 4. XỬ LÝ AUDIO FILES - CHỈ DÙNG WHISPER THẬT
        // ============================================================
        if (request.AudioFiles == null || request.AudioFiles.Count == 0)
        {
            throw new BadRequestException("No audio files provided");
        }

        Console.WriteLine($"🎤 Processing {request.AudioFiles.Count} audio files");

        for (int fileIndex = 0; fileIndex < request.AudioFiles.Count; fileIndex++)
        {
            var audioFile = request.AudioFiles[fileIndex];
            var question = questionList.ElementAtOrDefault(fileIndex);

            if (audioFile == null)
            {
                throw new BadRequestException($"Audio file at index {fileIndex} is null");
            }

            if (audioFile.Length == 0)
            {
                throw new BadRequestException($"Audio file at index {fileIndex} is empty (0 bytes)");
            }

            if (question == null)
            {
                throw new BadRequestException($"No question found for audio file at index {fileIndex}");
            }

            Console.WriteLine($"🎤 Audio file {fileIndex + 1}/{request.AudioFiles.Count}: {audioFile.Length} bytes");

            // ✅ UPLOAD FILE
            var audioUrl = await _fileStorageService.UploadFileAsync(audioFile, "speaking-audios");
            audioUrls.Add(audioUrl);
            Console.WriteLine($"✅ Uploaded: {audioUrl}");

            // ✅ ĐỌC FILE THÀNH BYTE ARRAY
            byte[] audioData;
            using (var ms = new MemoryStream())
            {
                await audioFile.CopyToAsync(ms, cancellationToken);
                audioData = ms.ToArray();
            }

            if (audioData == null || audioData.Length == 0)
            {
                throw new BadRequestException($"Audio data for question {question.OrderNumber} is empty after copy");
            }

            Console.WriteLine($"🎤 Audio data size: {audioData.Length} bytes for Q{question.OrderNumber}");

            // ✅ GỌI WHISPER API - NÉM EXCEPTION NẾU LỖI
            try
            {
                var transcript = await _aiGradingService.SpeechToTextAsync(audioData);

                if (string.IsNullOrWhiteSpace(transcript))
                {
                    throw new Exception($"Whisper returned empty transcript for Q{question.OrderNumber}");
                }

                transcripts[question.Id] = transcript;
                Console.WriteLine($"✅ Transcript for Q{question.OrderNumber}: {transcript.Substring(0, Math.Min(transcript.Length, 100))}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to transcribe Q{question.OrderNumber}: {ex.Message}");
                throw new Exception($"Failed to transcribe audio for question {question.OrderNumber}: {ex.Message}", ex);
            }
        }

        // ✅ KIỂM TRA TẤT CẢ CÂU HỎI ĐÃ CÓ TRANSCRIPT
        foreach (var question in questionList)
        {
            if (!transcripts.ContainsKey(question.Id))
            {
                throw new Exception($"No transcript found for question {question.OrderNumber}");
            }
        }

        // ============================================================
        // 5. CHẤM ĐIỂM TỪNG CÂU HỎI
        // ============================================================
        var details = new List<SubmissionAnswerDetailDto>();
        double totalScore = 0;

        foreach (var question in questionList)
        {
            var transcript = transcripts[question.Id];

            var aiResult = await _aiGradingService.GradeSpeakingContentAsync(transcript, question.QuestionText);

            if (aiResult == null)
            {
                throw new Exception($"AI grading failed for question {question.OrderNumber}");
            }

            totalScore += aiResult.TotalScore;

            details.Add(new SubmissionAnswerDetailDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                OrderNumber = question.OrderNumber,
                UserAnswer = transcript,
                CorrectAnswer = null,
                IsCorrect = aiResult.TotalScore >= 5.0,
                AiScore = aiResult.TotalScore,
                AiFeedback = FormatAiFeedback(aiResult),
                Explanation = null
            });
        }

        // ============================================================
        // 6. TÍNH ĐIỂM TRUNG BÌNH
        // ============================================================
        var averageScore = Math.Round(totalScore / questionList.Count, 1);
        averageScore = Math.Clamp(averageScore, 0, 10);

        // ============================================================
        // 7. TẠO SUBMISSION
        // ============================================================
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ExerciseId = request.ExerciseId,
            SkillType = 3,
            TotalScore = (short)Math.Round(averageScore),
            TotalQuestions = questionList.Count,
            CorrectCount = 0,
            TimeSpentSeconds = request.TimeSpentSeconds,
            AnswerJson = JsonSerializer.Serialize(transcripts),
            AudioUrl = audioUrls.Count > 0 ? string.Join(",", audioUrls) : null,
            Transcript = JsonSerializer.Serialize(transcripts),
            ResultJson = JsonSerializer.Serialize(details),
            AiFeedback = JsonSerializer.Serialize(details),
            SubmittedAt = DateTime.UtcNow,
            IsGraded = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Submissions.AddAsync(submission);

        // ============================================================
        // 8. CẬP NHẬT SESSION (NẾU CÓ)
        // ============================================================
        if (request.SessionId.HasValue)
        {
            var session = await _unitOfWork.FullTestSessions.GetByIdAsync(request.SessionId.Value);
            if (session != null)
            {
                session.SpeakingSubmissionId = submission.Id;
                session.SpeakingTimeSpent = request.TimeSpentSeconds;
                await _unitOfWork.FullTestSessions.UpdateAsync(session);
            }
        }

        // ============================================================
        // 9. CẬP NHẬT ATTEMPTCOUNT
        // ============================================================
        exercise.AttemptCount++;
        await _unitOfWork.Exercises.UpdateAsync(exercise);

        await _unitOfWork.SaveChangesAsync();

        // ============================================================
        // 10. TRẢ VỀ KẾT QUẢ
        // ============================================================
        return new SubmissionDetailDto
        {
            Id = submission.Id,
            ExerciseId = submission.ExerciseId,
            ExerciseTitle = exercise.Title,
            Skill = 3,
            SkillName = "Speaking",
            TotalScore = averageScore,
            TotalQuestions = questionList.Count,
            CorrectCount = 0,
            SubmittedAt = submission.SubmittedAt,
            Details = details,
            AiFeedback = new
            {
                Summary = "Speaking graded by AI",
                AudioUrls = audioUrls,
                Transcripts = transcripts
            }
        };
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    private static string FormatAiFeedback(SpeakingGradeResult result)
    {
        if (result == null) return string.Empty;
        return $"Điểm: {result.TotalScore}/10\n" +
               $"Điểm mạnh: {result.Strengths}\n" +
               $"Điểm yếu: {result.Weaknesses}\n" +
               $"Gợi ý: {result.Suggestions}";
    }
}