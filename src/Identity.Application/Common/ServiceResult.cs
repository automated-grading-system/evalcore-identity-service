namespace Identity.Application.Common;

public sealed class ServiceResult<T>
{
    private ServiceResult(bool isSuccess, T? value, ApiError? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public ApiError? Error { get; }

    public int StatusCode { get; }

    public static ServiceResult<T> Success(T value, int statusCode = 200)
    {
        return new ServiceResult<T>(isSuccess: true, value, error: null, statusCode);
    }

    public static ServiceResult<T> Failure(string code, string message, int statusCode, object? details = null)
    {
        return new ServiceResult<T>(
            isSuccess: false,
            value: default,
            new ApiError(code, message, details),
            statusCode);
    }
}
