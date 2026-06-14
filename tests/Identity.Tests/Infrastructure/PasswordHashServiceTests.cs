using Identity.Domain.Entities;
using Identity.Infrastructure.Security;

namespace Identity.Tests.Infrastructure;

public sealed class PasswordHashServiceTests
{
    [Fact]
    public void VerifyPassword_ReturnsTrue_ForCorrectPassword()
    {
        var now = DateTimeOffset.UtcNow;
        var account = Account.Create("Student Demo", "student@ags.local", "student", now);
        var passwordHashService = new PasswordHashService();
        account.SetPasswordHash(passwordHashService.HashPassword(account, "Password123!"), now);

        var verified = passwordHashService.VerifyPassword(account, "Password123!");

        Assert.True(verified);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForWrongPassword()
    {
        var now = DateTimeOffset.UtcNow;
        var account = Account.Create("Student Demo", "student@ags.local", "student", now);
        var passwordHashService = new PasswordHashService();
        account.SetPasswordHash(passwordHashService.HashPassword(account, "Password123!"), now);

        var verified = passwordHashService.VerifyPassword(account, "WrongPassword123!");

        Assert.False(verified);
    }
}
