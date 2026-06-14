using System.Text.Json.Serialization;

namespace Identity.Application.Common;

public sealed class ApiResponse<T>
{
    private ApiResponse(bool success, T? data, ApiError? error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public bool Success { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; }

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>(success: true, data, error: null);
    }

    public static ApiResponse<T> Fail(ApiError error)
    {
        return new ApiResponse<T>(success: false, data: default, error);
    }
}
