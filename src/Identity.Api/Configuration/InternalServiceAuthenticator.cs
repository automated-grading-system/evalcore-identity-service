using Microsoft.Extensions.Options;

namespace Identity.Api.Configuration;

public sealed class InternalServiceAuthenticator
{
    public const string ServiceHeader = "X-Internal-Service";
    public const string TokenHeader = "X-Internal-Service-Token";
    private const string NotificationService = "notification-service";

    private readonly InternalServiceAuthenticationOptions _options;

    public InternalServiceAuthenticator(IOptions<InternalServiceAuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public InternalServiceAuthenticationResult Authenticate(IHeaderDictionary headers)
    {
        var service = headers[ServiceHeader].ToString();
        var token = headers[TokenHeader].ToString();

        if (string.IsNullOrWhiteSpace(service) || string.IsNullOrWhiteSpace(token))
        {
            return InternalServiceAuthenticationResult.MissingHeaders;
        }

        return service == NotificationService
            && !string.IsNullOrWhiteSpace(_options.Token)
            && string.Equals(token, _options.Token, StringComparison.Ordinal)
            ? InternalServiceAuthenticationResult.Authenticated
            : InternalServiceAuthenticationResult.Forbidden;
    }
}

public enum InternalServiceAuthenticationResult
{
    Authenticated,
    MissingHeaders,
    Forbidden
}
