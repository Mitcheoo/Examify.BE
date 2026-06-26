// Examify.Application/Cqrs/Commands/Users/AssignRoleCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;
using Examify.Application.DTOs.Admin;
using Examify.Core.Exceptions;

namespace Examify.Application.Cqrs.Commands.Users;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, UserDto>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AssignRoleCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserDto> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        // ✅ CHỈ CHẤP NHẬN "User" VÀ "Admin"
        var validRoles = new[] { "User", "Admin" };
        var rolesToAssign = request.Dto.Roles
            .Where(r => validRoles.Contains(r))
            .ToList();

        if (!rolesToAssign.Any())
            throw new BadRequestException("Roles must be 'User' or 'Admin'");

        // Xóa tất cả roles hiện tại
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Gán roles mới
        foreach (var roleName in rolesToAssign)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new NotFoundException($"Role '{roleName}' does not exist");

            await _userManager.AddToRoleAsync(user, roleName);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles.ToList()
        };
    }
}