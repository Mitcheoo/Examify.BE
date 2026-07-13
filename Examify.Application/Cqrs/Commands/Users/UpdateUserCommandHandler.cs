// Examify.Application/Cqrs/Commands/Users/UpdateUserCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;
using Examify.Application.DTOs.Admin;
using Examify.Core.Exceptions;

namespace Examify.Application.Cqrs.Commands.Users;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UpdateUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null)
            throw new NotFoundException($"User with ID {request.Id} not found");

        // Cập nhật thông tin
        if (!string.IsNullOrEmpty(request.Dto.FullName))
            user.FullName = request.Dto.FullName;

        if (!string.IsNullOrEmpty(request.Dto.Email))
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Dto.Email);
            if (existingUser != null && existingUser.Id != user.Id)
                throw new BadRequestException($"Email '{request.Dto.Email}' already exists");

            user.Email = request.Dto.Email;
        }

        if (request.Dto.IsActive.HasValue)
            user.IsActive = request.Dto.IsActive.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        // ✅ CẬP NHẬT ROLES - CHỈ CHẤP NHẬN "User" VÀ "Admin"
        if (request.Dto.Roles != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var validRoles = new[] { "User", "Admin" };
            var rolesToAssign = request.Dto.Roles
                .Where(r => validRoles.Contains(r))
                .ToList();

            foreach (var roleName in rolesToAssign)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    throw new NotFoundException($"Role '{roleName}' does not exist");

                await _userManager.AddToRoleAsync(user, roleName);
            }

            // ✅ NẾU KHÔNG CÓ ROLE NÀO, GÁN MẶC ĐỊNH "User"
            if (!rolesToAssign.Any())
            {
                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));

                await _userManager.AddToRoleAsync(user, "User");
            }
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