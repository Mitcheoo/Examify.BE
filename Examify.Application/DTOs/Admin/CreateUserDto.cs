// Examify.Application/DTOs/Admin/CreateUserDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
}