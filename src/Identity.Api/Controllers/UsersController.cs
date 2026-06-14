using Identity.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[Authorize(Roles = "student,lecturer,admin")]
[Route("api/users")]
public sealed class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return UnauthorizedEnvelope();
        }

        var result = await _userService.GetCurrentUserAsync(userId, cancellationToken);
        return FromServiceResult(result);
    }
}
