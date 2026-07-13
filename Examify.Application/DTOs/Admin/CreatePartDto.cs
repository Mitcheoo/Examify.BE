// Examify.Application/DTOs/Admin/CreatePartDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreatePartDto
{
    public int PartNumber { get; set; }
    public string? Title { get; set; }
    public string? Passage { get; set; }
    public string? AudioUrl { get; set; }
}