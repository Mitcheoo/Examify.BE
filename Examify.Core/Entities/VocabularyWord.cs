// Examify.Core/Entities/VocabularyWord.cs
using System.ComponentModel.DataAnnotations;

namespace Examify.Core.Entities;

public class VocabularyWord : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Word { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Meaning { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Pronunciation { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Example { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PartOfSpeech { get; set; }

    [MaxLength(10)]
    public string Level { get; set; } = "B1"; // A1, A2, B1, B2, C1

    [MaxLength(50)]
    public string? Topic { get; set; }

    // Audio/Image URLs
    [MaxLength(500)]
    public string? AudioUrl { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // Additional fields
    [MaxLength(500)]
    public string? VietnameseExample { get; set; } // Dịch ví dụ sang tiếng Việt

    // Navigation
    public virtual ICollection<VocabularyProgress> Progress { get; set; } = new List<VocabularyProgress>();
}