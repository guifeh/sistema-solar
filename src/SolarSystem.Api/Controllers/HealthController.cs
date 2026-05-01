using Microsoft.AspNetCore.Mvc;

namespace SolarSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "Sistema Solar API",
            timestamp = DateTime.UtcNow
        });
    }
}
