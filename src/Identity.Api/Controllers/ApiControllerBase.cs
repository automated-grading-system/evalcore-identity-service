using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.Application.Common;
using Identity.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromServiceResult<T>(ServiceResult<T> result, int? successStatusCode = null)
    {
        if (result.IsSuccess)
        {
            return StatusCode(successStatusCode ?? result.StatusCode, ApiResponse<T>.Ok(result.Value!));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Error!));
    }

    protected IActionResult UnauthorizedEnvelope()
    {
        return Unauthorized(ApiResponse<object>.Fail(new ApiError(
            ErrorCodes.Unauthorized,
            "Unauthorized")));
    }

    protected bool TryGetCurrentUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(subject, out userId);
    }
}
