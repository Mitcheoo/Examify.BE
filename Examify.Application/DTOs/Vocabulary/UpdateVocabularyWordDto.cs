// Examify.Application/DTOs/Vocabulary/UpdateVocabularyWordDto.cs
using System.ComponentModel.DataAnnotations;

namespace Examify.Application.DTOs.Vocabulary;

public class UpdateVocabularyWordDto
{
    [MaxLength(100)]
    public string? Word { get; set; }

    [MaxLength(500)]
    public string? Meaning { get; set; }

    [MaxLength(50)]
    public string? Pronunciation { get; set; }

    [MaxLength(1000)]
    public string? Example { get; set; }  // ✅ Cho phép null

    [MaxLength(50)]
    public string? PartOfSpeech { get; set; }

    [MaxLength(10)]
    public string? Level { get; set; }

    [MaxLength(50)]
    public string? Topic { get; set; }

    [MaxLength(500)]
    public string? AudioUrl { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? VietnameseExample { get; set; }
}