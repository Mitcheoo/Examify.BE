// Examify.API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Commands.Auth;
using Examify.Application.DTOs.Auth;
using Examify.Core.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public AuthController(IMediator mediator, UserManager<User> userManager, IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
    }

    // ========== AUTHENTICATION ==========

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var command = new LoginCommand(request.UserName, request.Password);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<bool>> Register(RegisterRequestDto request)
    {
        var command = new RegisterCommand(request.FullName, request.UserName, request.Email, request.Password);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ========== PROFILE MANAGEMENT ==========

    /// <summary>
    /// Lấy thông tin profile của user hiện tại
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Cập nhật thông tin profile
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        if (!string.IsNullOrEmpty(model.FullName))
            user.FullName = model.FullName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Profile updated successfully" });
    }

    /// <summary>
    /// Đổi mật khẩu (khi đã đăng nhập)
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Upload avatar
    /// </summary>
    [Authorize]
    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User not authenticated" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        if (avatarFile == null || avatarFile.Length == 0)
            return BadRequest(new { message = "Avatar file is required" });

        // Kiểm tra định dạng file
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(new { message = "Only JPG, PNG, GIF, WEBP files are allowed" });

        // Giới hạn kích thước (2MB)
        if (avatarFile.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "File size must be less than 2MB" });

        // Upload file
        var avatarUrl = await _fileStorageService.UploadFileAsync(avatarFile, "avatars");

        // Xóa avatar cũ nếu có
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            await _fileStorageService.DeleteFileAsync(user.AvatarUrl);
        }

        // Cập nhật database
        user.AvatarUrl = avatarUrl;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { avatarUrl, message = "Avatar uploaded successfully" });
    }

    // ========== FORGOT / RESET PASSWORD ==========

    /// <summary>
    /// Quên mật khẩu - gửi email reset
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Không tiết lộ user có tồn tại hay không (bảo mật)
            return Ok(new { message = "If your email is registered, you will receive a reset link." });
        }

        // Tạo token reset password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Tạo link reset (gửi qua email - cần tích hợp email service)
        var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={Uri.EscapeDataString(token)}&email={model.Email}";

        // TODO: Gửi email chứa resetLink
        // await _emailService.SendResetPasswordEmailAsync(user.Email, resetLink);

        return Ok(new { message = "If your email is registered, you will receive a reset link." });
    }

    /// <summary>
    /// Đặt lại mật khẩu (sau khi nhận link email)
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid request" });

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Password has been reset successfully" });
    }
}