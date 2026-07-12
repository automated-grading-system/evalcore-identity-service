using Identity.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Identity.Tests.Api;

public sealed class InternalServiceAuthenticatorTests
{
    private readonly InternalServiceAuthenticator _authenticator = new(
        Options.Create(new InternalServiceAuthenticationOptions { Token = "test-internal-token" }));

    [Fact]
    public void Authenticate_ReturnsMissingHeaders_WhenEitherHeaderIsMissing()
    {
        var headers = new HeaderDictionary();

        var result = _authenticator.Authenticate(headers);

        Assert.Equal(InternalServiceAuthenticationResult.MissingHeaders, result);
    }

    [Fact]
    public void Authenticate_ReturnsForbidden_WhenServiceOrTokenIsWrong()
    {
        var headers = new HeaderDictionary
        {
            [InternalServiceAuthenticator.ServiceHeader] = "other-service",
            [InternalServiceAuthenticator.TokenHeader] = "wrong"
        };

        var result = _authenticator.Authenticate(headers);

        Assert.Equal(InternalServiceAuthenticationResult.Forbidden, result);
    }

    [Fact]
    public void Authenticate_ReturnsAuthenticated_ForNotificationServiceAndValidToken()
    {
        var headers = new HeaderDictionary
        {
            [InternalServiceAuthenticator.ServiceHeader] = "notification-service",
            [InternalServiceAuthenticator.TokenHeader] = "test-internal-token"
        };

        var result = _authenticator.Authenticate(headers);

        Assert.Equal(InternalServiceAuthenticationResult.Authenticated, result);
    }
}
