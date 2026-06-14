using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Security;

public sealed class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public string HashPassword(Account account, string password)
    {
        return _passwordHasher.HashPassword(account, password);
    }

    public bool VerifyPassword(Account account, string password)
    {
        if (string.IsNullOrWhiteSpace(account.PasswordHash))
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
