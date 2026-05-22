// Examify.Application/DTOs/Submissions/SubmissionResultDto.cs
namespace Examify.Application.DTOs.Submissions;

public class SubmissionResultDto
{
    public Guid SubmissionId { get; set; }
    public short TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime SubmittedAt { get; set; }
}