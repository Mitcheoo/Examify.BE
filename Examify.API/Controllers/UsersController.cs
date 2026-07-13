// Examify.API/Controllers/Admin/UsersController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Examify.Application.Cqrs.Commands.Users;
using Examify.Application.Cqrs.Queries.Users;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Common;
using System.Security.Claims;

namespace Examify.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách users (có phân trang và tìm kiếm)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var query = new GetUsersQuery(page, pageSize, search);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy user theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Tạo user mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        var command = new CreateUserCommand(dto);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
    }

    /// <summary>
    /// Cập nhật user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserDto dto)
    {
        var command = new UpdateUserCommand(id, dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Xóa user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteUser(Guid id)
    {
        // Không cho xóa chính mình
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == id.ToString())
            return BadRequest(new { message = "Cannot delete yourself" });

        var command = new DeleteUserCommand(id);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Gán role cho user
    /// </summary>
    [HttpPost("{id}/roles")]
    public async Task<ActionResult<UserDto>> AssignRoles(Guid id, AssignRoleDto dto)
    {
        var command = new AssignRoleCommand(id, dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}