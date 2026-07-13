// Examify.Application/DTOs/Admin/UpdateUserDto.cs
namespace Examify.Application.DTOs.Admin;

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Roles { get; set; }
}