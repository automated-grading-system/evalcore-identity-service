namespace Identity.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "ags";

    public string Audience { get; set; } = "ags-api";

    public int ExpiresMinutes { get; set; } = 1440;
}
