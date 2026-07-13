// Examify.Application/DTOs/Vocabulary/CreateVocabularyWordDto.cs
using System.ComponentModel.DataAnnotations;

namespace Examify.Application.DTOs.Vocabulary;

public class CreateVocabularyWordDto
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
    public string Level { get; set; } = "B1";

    [MaxLength(50)]
    public string? Topic { get; set; }

    [MaxLength(500)]
    public string? AudioUrl { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? VietnameseExample { get; set; }
}