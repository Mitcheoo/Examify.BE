// Examify.Core/Entities/VocabularyTopic.cs (Tùy chọn - để quản lý chủ đề)
using System.ComponentModel.DataAnnotations;

namespace Examify.Core.Entities;

public class VocabularyTopic : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int WordCount { get; set; } = 0;

    // Navigation
    public virtual ICollection<VocabularyWord> Words { get; set; } = new List<VocabularyWord>();
}