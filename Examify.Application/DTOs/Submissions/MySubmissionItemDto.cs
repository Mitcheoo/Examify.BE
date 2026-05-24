// Examify.Application/DTOs/Submissions/MySubmissionItemDto.cs
namespace Examify.Application.DTOs.Submissions;

public class MySubmissionItemDto
{
    public Guid SubmissionId { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public short TotalScore { get; set; }
    public DateTime SubmittedAt { get; set; }
}