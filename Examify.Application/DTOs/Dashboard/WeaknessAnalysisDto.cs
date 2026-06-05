// Examify.Application/DTOs/Dashboard/WeaknessAnalysisDto.cs
namespace Examify.Application.DTOs.Dashboard;

public class WeaknessAnalysisDto
{
    public List<SkillWeaknessDto> Weaknesses { get; set; } = new();
    public List<RecommendationDto> Recommendations { get; set; } = new();
}

public class SkillWeaknessDto
{
    public string Skill { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Level { get; set; } = string.Empty;  // Yếu, Trung bình, Tốt
    public string Message { get; set; } = string.Empty;
}

public class RecommendationDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SuggestedExercises { get; set; } = string.Empty;
    public int Priority { get; set; }  // 1: Cao, 2: Trung bình, 3: Thấp
}