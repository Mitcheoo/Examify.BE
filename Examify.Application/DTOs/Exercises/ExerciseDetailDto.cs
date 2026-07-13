// Examify.Application/DTOs/Exercises/ExerciseDetailDto.cs
namespace Examify.Application.DTOs.Exercises;

public class ExerciseDetailDto
{
    public Guid Id { get; set; }
    public int Skill { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AudioUrl { get; set; }
    public string? Passage { get; set; }
    public string? PassagesJson { get; set; }
    public int TotalParts { get; set; }
    public int TotalQuestions { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int Difficulty { get; set; }
    public bool IsFullTest { get; set; }
    public int AttemptCount { get; set; }
    public bool IsPurchased { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFree { get; set; }
    public decimal Price { get; set; }

    // 4 PROPERTY LIÊN KẾT CHO FULL TEST
    public Guid? ReadingExerciseId { get; set; }
    public Guid? ListeningExerciseId { get; set; }
    public Guid? WritingExerciseId { get; set; }
    public Guid? SpeakingExerciseId { get; set; }

    // ✅ THÊM 3 PROPERTY NÀY CHO CÂU HỎI
    public List<QuestionDto> Questions { get; set; } = new();
    public List<WritingQuestionDto> WritingQuestions { get; set; } = new();
    public List<SpeakingQuestionDto> SpeakingQuestions { get; set; } = new();
}