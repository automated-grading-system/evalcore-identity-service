using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityDatabaseSeeder
{
    private const string DemoPassword = "Password123!";

    private static readonly SeedAccount[] SeedAccounts =
    [
        new("Admin Demo", "admin@ags.local", "admin"),
        new("Lecturer Demo", "lecturer@ags.local", "lecturer"),
        new("Student Demo", "student@ags.local", "student")
    ];

    private readonly IdentityDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IPasswordHashService _passwordHashService;

    public IdentityDatabaseSeeder(
        IdentityDbContext dbContext,
        IPasswordHashService passwordHashService,
        IClock clock)
    {
        _dbContext = dbContext;
        _passwordHashService = passwordHashService;
        _clock = clock;
    }

    public async Task SeedDevelopmentAccountsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var seedAccount in SeedAccounts)
        {
            var normalizedEmail = Account.NormalizeEmail(seedAccount.Email);
            var exists = await _dbContext.Accounts.AnyAsync(
                account => account.Email == normalizedEmail,
                cancellationToken);

            if (exists)
            {
                continue;
            }

            var now = _clock.UtcNow;
            var account = Account.Create(seedAccount.FullName, normalizedEmail, seedAccount.Role, now);
            account.SetPasswordHash(_passwordHashService.HashPassword(account, DemoPassword), now);

            _dbContext.Accounts.Add(account);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record SeedAccount(string FullName, string Email, string Role);
}
