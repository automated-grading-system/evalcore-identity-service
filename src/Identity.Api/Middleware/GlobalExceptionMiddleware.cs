using Identity.Api.Serialization;
using Identity.Application.Common;
using Identity.Application.Errors;

namespace Identity.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            _logger.LogError(exception, "Unhandled request failure");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(new ApiError(
                ErrorCodes.InternalError,
                "An unexpected error occurred"));

            await context.Response.WriteAsJsonAsync(response, ApiJsonOptions.SerializerOptions);
        }
    }
}
