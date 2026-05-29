// Examify.API/Controllers/HealthController.cs
using Microsoft.AspNetCore.Mvc;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            message = "Examify API is running!"
        });
    }
}