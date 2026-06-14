namespace Identity.Application.Dtos;

public sealed class AccessTokenResult
{
    public AccessTokenResult(string accessToken, DateTimeOffset expiresAt)
    {
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
    }

    public string AccessToken { get; }

    public DateTimeOffset ExpiresAt { get; }
}
