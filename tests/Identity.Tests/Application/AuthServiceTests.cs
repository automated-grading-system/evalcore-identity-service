using Identity.Application.Abstractions;
using Identity.Application.Common;
using Identity.Application.Dtos;
using Identity.Application.Errors;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Infrastructure.Security;

namespace Identity.Tests.Application;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var clock = new FakeClock();
        var repository = new InMemoryAccountRepository();
        var passwordHashService = new PasswordHashService();
        var existing = CreateAccount("Student Demo", "student@ags.local", "student", passwordHashService, clock);
        await repository.AddAsync(existing);
        var service = CreateService(repository, passwordHashService, clock);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            FullName = "Another Student",
            Email = "STUDENT@ags.local",
            Password = "Password123!",
            Role = "student"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal(ErrorCodes.EmailAlreadyExists, result.Error?.Code);
    }

    [Fact]
    public async Task LoginAsync_ReturnsAccountLocked_WhenPasswordIsCorrectAndAccountIsLocked()
    {
        var clock = new FakeClock();
        var repository = new InMemoryAccountRepository();
        var passwordHashService = new PasswordHashService();
        var account = CreateAccount("Student Demo", "student@ags.local", "student", passwordHashService, clock);
        account.Lock(clock.UtcNow);
        await repository.AddAsync(account);
        var service = CreateService(repository, passwordHashService, clock);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "student@ags.local",
            Password = "Password123!"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal(ErrorCodes.AccountLocked, result.Error?.Code);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        var clock = new FakeClock();
        var repository = new InMemoryAccountRepository();
        var passwordHashService = new PasswordHashService();
        var account = CreateAccount("Student Demo", "student@ags.local", "student", passwordHashService, clock);
        await repository.AddAsync(account);
        var service = CreateService(repository, passwordHashService, clock);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "student@ags.local",
            Password = "Password123!"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("test-token", result.Value?.AccessToken);
        Assert.Equal(account.Id, result.Value?.User.Id);
    }

    private static AuthService CreateService(
        IAccountRepository repository,
        IPasswordHashService passwordHashService,
        IClock clock)
    {
        return new AuthService(repository, passwordHashService, new FakeJwtTokenProvider(clock), clock);
    }

    private static Account CreateAccount(
        string fullName,
        string email,
        string role,
        IPasswordHashService passwordHashService,
        IClock clock)
    {
        var account = Account.Create(fullName, email, role, clock.UtcNow);
        account.SetPasswordHash(passwordHashService.HashPassword(account, "Password123!"), clock.UtcNow);
        return account;
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 6, 14, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeJwtTokenProvider : IJwtTokenProvider
    {
        private readonly IClock _clock;

        public FakeJwtTokenProvider(IClock clock)
        {
            _clock = clock;
        }

        public AccessTokenResult CreateAccessToken(Account account)
        {
            return new AccessTokenResult("test-token", _clock.UtcNow.AddMinutes(1440));
        }
    }

    private sealed class InMemoryAccountRepository : IAccountRepository
    {
        private readonly List<Account> _accounts = [];

        public Task AddAsync(Account account, CancellationToken cancellationToken = default)
        {
            _accounts.Add(account);
            return Task.CompletedTask;
        }

        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_accounts.Any(account => account.Email == normalizedEmail));
        }

        public Task<Account?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_accounts.FirstOrDefault(account => account.Email == normalizedEmail));
        }

        public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_accounts.FirstOrDefault(account => account.Id == id));
        }

        public Task<PagedResult<Account>> ListAsync(
            AccountQuery query,
            CancellationToken cancellationToken = default)
        {
            var accounts = _accounts
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToArray();

            return Task.FromResult(new PagedResult<Account>(accounts, _accounts.Count));
        }

        public Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
