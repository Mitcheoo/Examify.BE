// Examify.Application/Cqrs/Commands/Auth/LoginCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Auth;

namespace Examify.Application.Cqrs.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Tìm user theo username hoặc email
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(request.UserName);
        }

        

        // Kiểm tra mật khẩu
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

         

        // Lấy roles của user
        var roles = await _userManager.GetRolesAsync(user);

        // Tạo token
        var token = _tokenService.GenerateToken(user, roles.ToList());

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = roles.ToList()
        };
    }
}