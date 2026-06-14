namespace Identity.Application.Dtos;

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required LoginUserResponse User { get; init; }
}
