// Examify.Application/DTOs/Auth/LoginRequestDto.cs
namespace Examify.Application.DTOs.Auth;

public class LoginRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}