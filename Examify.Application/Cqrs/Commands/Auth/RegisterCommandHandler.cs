// Examify.Application/Cqrs/Commands/Auth/RegisterCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;

namespace Examify.Application.Cqrs.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra user đã tồn tại chưa
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser != null)
            throw new Exception("Username already exists");

        existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new Exception("Email already exists");

        // Tạo user mới
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            UserName = request.UserName,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Registration failed: {errors}");
        }

        // ✅ Kiểm tra role "User" tồn tại, nếu không thì tạo
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));
        }

        // Gán role mặc định là "User"
        await _userManager.AddToRoleAsync(user, "User");

        return true;
    }
}