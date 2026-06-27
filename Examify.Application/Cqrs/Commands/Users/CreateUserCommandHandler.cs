// Examify.Application/Cqrs/Commands/Users/CreateUserCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;
using Examify.Application.DTOs.Admin;
using Examify.Core.Exceptions;

namespace Examify.Application.Cqrs.Commands.Users;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public CreateUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra email đã tồn tại
        var existingUser = await _userManager.FindByEmailAsync(request.Dto.Email);
        if (existingUser != null)
            throw new BadRequestException($"Email '{request.Dto.Email}' already exists");

        // Kiểm tra username đã tồn tại
        existingUser = await _userManager.FindByNameAsync(request.Dto.UserName);
        if (existingUser != null)
            throw new BadRequestException($"Username '{request.Dto.UserName}' already exists");

        var user = new User
        {
            UserName = request.Dto.UserName,
            Email = request.Dto.Email,
            FullName = request.Dto.FullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Dto.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        // ✅ CHỈ GÁN ROLE User (mặc định)
        // Nếu request gửi lên roles, chỉ chấp nhận "User" hoặc "Admin"
        var rolesToAssign = new List<string>();

        if (request.Dto.Roles != null && request.Dto.Roles.Any())
        {
            foreach (var roleName in request.Dto.Roles)
            {
                // ✅ KIỂM TRA ROLE HỢP LỆ
                var validRoles = new[] { "User", "Admin" };
                if (!validRoles.Contains(roleName))
                    continue;

                if (!await _roleManager.RoleExistsAsync(roleName))
                    throw new NotFoundException($"Role '{roleName}' does not exist");

                rolesToAssign.Add(roleName);
            }
        }

        // ✅ MẶC ĐỊNH GÁN ROLE "User" NẾU CHƯA CÓ
        if (!rolesToAssign.Any())
        {
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));

            rolesToAssign.Add("User");
        }

        foreach (var roleName in rolesToAssign)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles.ToList()
        };
    }
}