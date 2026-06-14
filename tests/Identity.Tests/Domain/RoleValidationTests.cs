using Identity.Domain.Constants;

namespace Identity.Tests.Domain;

public sealed class RoleValidationTests
{
    [Theory]
    [InlineData("student", "student")]
    [InlineData("lecturer", "lecturer")]
    [InlineData("admin", "admin")]
    [InlineData("Student", "student")]
    public void TryNormalize_AcceptsSupportedRoles(string input, string expected)
    {
        var result = AccountRoles.TryNormalize(input, out var normalized);

        Assert.True(result);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("owner")]
    [InlineData("")]
    [InlineData(null)]
    public void TryNormalize_RejectsUnsupportedRoles(string? input)
    {
        var result = AccountRoles.TryNormalize(input, out var normalized);

        Assert.False(result);
        Assert.Equal(string.Empty, normalized);
    }
}
