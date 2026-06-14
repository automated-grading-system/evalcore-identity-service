using Identity.Application.Abstractions;
using Identity.Application.Dtos;
using Identity.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[Authorize(Roles = AccountRoles.Admin)]
[Route("api/admin/users")]
public sealed class AdminUsersController : ApiControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<IActionResult> ListUsers(
        [FromQuery] AdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _adminUserService.ListUsersAsync(query, cancellationToken);
        return FromServiceResult(result);
    }

    [HttpPatch("{userId:guid}/lock")]
    public async Task<IActionResult> LockUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentAdminId))
        {
            return UnauthorizedEnvelope();
        }

        var result = await _adminUserService.LockUserAsync(userId, currentAdminId, cancellationToken);
        return FromServiceResult(result);
    }

    [HttpPatch("{userId:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _adminUserService.UnlockUserAsync(userId, cancellationToken);
        return FromServiceResult(result);
    }
}
