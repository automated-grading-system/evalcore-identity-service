using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[AllowAnonymous]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new HealthResponse(
            "healthy",
            "identity-service",
            DateTimeOffset.UtcNow));
    }

    private sealed record HealthResponse(string Status, string Service, DateTimeOffset Timestamp);
}
