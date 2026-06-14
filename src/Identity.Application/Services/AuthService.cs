using Identity.Application.Abstractions;
using Identity.Application.Common;
using Identity.Application.Dtos;
using Identity.Application.Errors;
using Identity.Application.Exceptions;
using Identity.Domain.Constants;
using Identity.Domain.Entities;

namespace Identity.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IClock _clock;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IPasswordHashService _passwordHashService;

    public AuthService(
        IAccountRepository accountRepository,
        IPasswordHashService passwordHashService,
        IJwtTokenProvider jwtTokenProvider,
        IClock clock)
    {
        _accountRepository = accountRepository;
        _passwordHashService = passwordHashService;
        _jwtTokenProvider = jwtTokenProvider;
        _clock = clock;
    }

    public async Task<ServiceResult<AccountResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!AccountRoles.TryNormalize(request.Role, out var normalizedRole))
        {
            return ServiceResult<AccountResponse>.Failure(
                ErrorCodes.ValidationError,
                "Validation failed",
                400,
                new Dictionary<string, string[]>
                {
                    ["role"] = ["Role must be one of: student, lecturer, admin"]
                });
        }

        var normalizedEmail = Account.NormalizeEmail(request.Email);

        if (await _accountRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return EmailAlreadyExists();
        }

        var now = _clock.UtcNow;
        var account = Account.Create(request.FullName, normalizedEmail, normalizedRole, now);
        var passwordHash = _passwordHashService.HashPassword(account, request.Password);
        account.SetPasswordHash(passwordHash, now);

        try
        {
            await _accountRepository.AddAsync(account, cancellationToken);
        }
        catch (DuplicateEmailException)
        {
            return EmailAlreadyExists();
        }

        return ServiceResult<AccountResponse>.Success(MapAccount(account), 201);
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = Account.NormalizeEmail(request.Email);
        var account = await _accountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (account is null || !_passwordHashService.VerifyPassword(account, request.Password))
        {
            return ServiceResult<LoginResponse>.Failure(
                ErrorCodes.InvalidCredentials,
                "Invalid email or password",
                401);
        }

        if (account.IsLocked)
        {
            return ServiceResult<LoginResponse>.Failure(
                ErrorCodes.AccountLocked,
                "Account is locked",
                403);
        }

        var token = _jwtTokenProvider.CreateAccessToken(account);

        var response = new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            User = new LoginUserResponse
            {
                Id = account.Id,
                FullName = account.FullName,
                Email = account.Email,
                Role = account.Role
            }
        };

        return ServiceResult<LoginResponse>.Success(response);
    }

    private static ServiceResult<AccountResponse> EmailAlreadyExists()
    {
        return ServiceResult<AccountResponse>.Failure(
            ErrorCodes.EmailAlreadyExists,
            "Email already exists",
            409);
    }

    private static AccountResponse MapAccount(Account account)
    {
        return new AccountResponse
        {
            Id = account.Id,
            FullName = account.FullName,
            Email = account.Email,
            Role = account.Role,
            IsLocked = account.IsLocked,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }
}
