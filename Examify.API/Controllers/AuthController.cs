// Examify.API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Commands.Auth;
using Examify.Application.DTOs.Auth;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
}