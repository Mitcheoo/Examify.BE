// Examify.Application/Cqrs/Commands/Auth/LoginCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Auth;
using Examify.Application.DTOs;

namespace Examify.Application.Cqrs.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ResponseDto>
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

    public async Task<ResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(request.UserName);
        }

        if (user == null)
        {
            return new ResponseDto
            {
                Success = false,
                Message = "Invalid username or password",
                Errors = new[] { "User not found" }
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            return new ResponseDto
            {
                Success = false,
                Message = "Invalid username or password",
                Errors = new[] { "Invalid credentials" }
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles.ToList());

        var loginResponse = new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Roles = roles.ToList(),
            ExpiresIn = 86400  
        };

        return new ResponseDto
        {
            Success = true,
            Message = "Login successful",
            Data = loginResponse
        };
    }
}