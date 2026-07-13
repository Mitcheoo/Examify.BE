// Examify.Application/Cqrs/Commands/Users/DeleteUserCommandHandler.cs
using MediatR;
using Microsoft.AspNetCore.Identity;
using Examify.Core.Entities;
using Examify.Core.Exceptions;

namespace Examify.Application.Cqrs.Commands.Users;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly UserManager<User> _userManager;

    public DeleteUserCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null)
            throw new NotFoundException($"User with ID {request.Id} not found");

        // Không cho xóa chính mình
        // Cần truyền userId từ controller để kiểm tra (sẽ xử lý ở controller)

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        return true;
    }
}