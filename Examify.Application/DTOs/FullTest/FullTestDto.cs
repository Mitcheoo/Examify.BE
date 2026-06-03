// Examify.Application/DTOs/FullTest/FullTestDto.cs
namespace Examify.Application.DTOs.FullTest;

public class StartFullTestResponse
{
    public Guid SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public List<FullTestPartDto> Parts { get; set; } = new();
}

public class FullTestPartDto
{
    public int PartNumber { get; set; }
    public string PartName { get; set; } = string.Empty;
    public Guid ExerciseId { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsCompleted { get; set; }
}

public class SavePartRequest
{
    public Guid SessionId { get; set; }
    public int PartNumber { get; set; }
    public Dictionary<Guid, string> Answers { get; set; } = new();
    public string? EssayText { get; set; }
    public string? AudioUrl { get; set; }
    public int TimeSpentSeconds { get; set; }
}

public class FullTestResultResponse
{
    public Guid SessionId { get; set; }
    public short TotalScore { get; set; }
    public short ReadingScore { get; set; }
    public short ListeningScore { get; set; }
    public short WritingScore { get; set; }
    public short SpeakingScore { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalTimeSpentSeconds { get; set; }
}