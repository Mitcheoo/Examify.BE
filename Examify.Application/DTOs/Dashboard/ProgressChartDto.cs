// Examify.Application/DTOs/Dashboard/ProgressChartDto.cs
namespace Examify.Application.DTOs.Dashboard;

public class ProgressChartDto
{
    public List<string> Labels { get; set; } = new();  // Tuần/Tháng
    public List<ChartDataset> Datasets { get; set; } = new();
}

public class ChartDataset
{
    public string Label { get; set; } = string.Empty;
    public List<double> Data { get; set; } = new();
    public string BorderColor { get; set; } = "#667eea";
    public string BackgroundColor { get; set; } = "rgba(102,126,234,0.1)";
    public bool Fill { get; set; } = true;
    public int BorderWidth { get; set; } = 2;
}