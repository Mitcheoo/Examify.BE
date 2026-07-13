// Examify.Application/DTOs/Vocabulary/VocabularyFilterDto.cs
namespace Examify.Application.DTOs.Vocabulary;

public class VocabularyFilterDto
{
    public string? Search { get; set; }
    public string? Level { get; set; } // A1, A2, B1, B2, C1
    public string? Topic { get; set; }
    public string? Status { get; set; } // all, mastered, learning, need_review
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } // word, level, created_at, review_count
    public bool SortDescending { get; set; } = false;
}