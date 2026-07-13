// Examify.Application/DTOs/Vocabulary/VocabularyProgressDto.cs
namespace Examify.Application.DTOs.Vocabulary;

public class VocabularyProgressDto
{
    public Guid VocabularyWordId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Pronunciation { get; set; }
    public bool IsMastered { get; set; }
    public int ReviewCount { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public DateTime? NextReviewAt { get; set; }
    public int StreakCount { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public double MasteryPercentage { get; set; } // % thành thạo dựa trên số lần review
}