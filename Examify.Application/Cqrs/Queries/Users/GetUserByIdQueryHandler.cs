// Examify.Application/Cqrs/Queries/Users/GetUserByIdQueryHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;
using Examify.Application.DTOs.Admin;
using Examify.Core.Exceptions;

namespace Examify.Application.Cqrs.Queries.Users;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly UserManager<User> _userManager;

    public GetUserByIdQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null)
            throw new NotFoundException($"User with ID {request.Id} not found");

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