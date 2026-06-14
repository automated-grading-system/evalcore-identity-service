namespace Identity.Application.Common;

public sealed class ApiError
{
    public ApiError(string code, string message, object? details = null)
    {
        Code = code;
        Message = message;
        Details = details ?? new Dictionary<string, string[]>();
    }

    public string Code { get; }

    public string Message { get; }

    public object Details { get; }
}
