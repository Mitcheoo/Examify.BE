// Examify.Application/DTOs/FullTest/FullTestStatusDto.cs
namespace Examify.Application.DTOs.FullTest;

/// <summary>
/// Trạng thái chi tiết của Full Test
/// </summary>
public class FullTestStatusDto
{
    public Guid FullTestId { get; set; }
    public string FullTestTitle { get; set; } = string.Empty;
    public List<SkillStatusDto> Skills { get; set; } = new();
}

/// <summary>
/// Trạng thái của từng kỹ năng trong Full Test
/// </summary>
public class SkillStatusDto
{
    public int Skill { get; set; }          // 0: Reading, 1: Listening, 2: Writing, 3: Speaking
    public string SkillName { get; set; } = string.Empty;
    public Guid? ExerciseId { get; set; }   // ID của bài thi kỹ năng
    public bool IsUnlocked { get; set; }    // Đã mở khóa chưa?
    public bool IsCompleted { get; set; }   // Đã làm xong chưa?
    public int Attempts { get; set; }       // Số lần đã làm
    public double? BestScore { get; set; }  // Điểm cao nhất (nếu có)
    public double? LatestScore { get; set; } // Điểm lần gần nhất (nếu có)
    public DateTime? LastAttemptAt { get; set; } // Thời gian làm gần nhất
    public int? RequiredSkill { get; set; } // Kỹ năng yêu cầu để mở khóa
    public string? RequiredSkillName { get; set; }
    public string? Message { get; set; }    // Thông báo điều kiện mở khóa
}