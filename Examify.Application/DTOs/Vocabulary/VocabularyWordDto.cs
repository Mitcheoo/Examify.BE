// Examify.Application/DTOs/Vocabulary/VocabularyWordDto.cs
namespace Examify.Application.DTOs.Vocabulary;

public class VocabularyWordDto
{
    public Guid Id { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Pronunciation { get; set; }
    public string Example { get; set; } = string.Empty;
    public string? PartOfSpeech { get; set; }
    public string Level { get; set; } = "B1";
    public string? Topic { get; set; }
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? VietnameseExample { get; set; }
    public DateTime CreatedAt { get; set; }

    // Progress info (if logged in)
    public bool IsMastered { get; set; }
    public int ReviewCount { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public DateTime? NextReviewAt { get; set; }
}