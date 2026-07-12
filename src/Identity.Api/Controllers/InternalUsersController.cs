using Identity.Api.Configuration;
using Identity.Application.Abstractions;
using Identity.Application.Common;
using Identity.Application.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/internal/users")]
public sealed class InternalUsersController : ControllerBase
{
    private readonly IInternalUserService _userService;
    private readonly InternalServiceAuthenticator _authenticator;

    public InternalUsersController(
        IInternalUserService userService,
        InternalServiceAuthenticator authenticator)
    {
        _userService = userService;
        _authenticator = authenticator;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUser(Guid userId, CancellationToken cancellationToken)
    {
        var authentication = _authenticator.Authenticate(Request.Headers);
        if (authentication == InternalServiceAuthenticationResult.MissingHeaders)
        {
            return Unauthorized(ApiResponse<object>.Fail(new ApiError(ErrorCodes.Unauthorized, "Unauthorized")));
        }

        if (authentication == InternalServiceAuthenticationResult.Forbidden)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(new ApiError(ErrorCodes.Forbidden, "Forbidden")));
        }

        var result = await _userService.GetInternalUserAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Error!));
    }
}
