// Examify.Application/DTOs/FullTest/FullTestDto.cs
namespace Examify.Application.DTOs.FullTest;

public class StartFullTestResponse
{
    public Guid SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public List<FullTestPartDto> Parts { get; set; } = [];
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

public class SubmitFullTestRequest
{
    public Guid SessionId { get; set; }
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

public class FullTestResultDetailDto
{
    public Guid SessionId { get; set; }
    public Guid FullTestId { get; set; }
    public string FullTestTitle { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalTimeSpentSeconds { get; set; }
    public short TotalScore { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public DateTime SubmittedAt { get; set; }
    public List<SkillResultDto> SkillResults { get; set; } = [];
}

public class SkillResultDto
{
    public int Skill { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public Guid? SubmissionId { get; set; }
    public short Score { get; set; }
    public int TimeSpentSeconds { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsCompleted { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public string? Status { get; set; }
}

public class FullTestStatusDto
{
    public Guid FullTestId { get; set; }
    public string FullTestTitle { get; set; } = string.Empty;
    public List<SkillStatusDto> Skills { get; set; } = [];
}

public class SkillStatusDto
{
    public int Skill { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public Guid? ExerciseId { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsCompleted { get; set; }
    public int Attempts { get; set; }
    public double? BestScore { get; set; }
    public double? LatestScore { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public int? RequiredSkill { get; set; }
    public string? RequiredSkillName { get; set; }
    public string? Message { get; set; }
}