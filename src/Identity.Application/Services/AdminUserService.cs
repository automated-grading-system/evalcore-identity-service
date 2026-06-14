using Identity.Application.Abstractions;
using Identity.Application.Common;
using Identity.Application.Dtos;
using Identity.Application.Errors;
using Identity.Domain.Constants;
using Identity.Domain.Entities;

namespace Identity.Application.Services;

public sealed class AdminUserService : IAdminUserService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IAccountRepository _accountRepository;
    private readonly IClock _clock;

    public AdminUserService(IAccountRepository accountRepository, IClock clock)
    {
        _accountRepository = accountRepository;
        _clock = clock;
    }

    public async Task<ServiceResult<PagedResponse<AccountResponse>>> ListUsersAsync(
        AdminUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        string? normalizedRole = null;

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            if (!AccountRoles.TryNormalize(query.Role, out var role))
            {
                return ServiceResult<PagedResponse<AccountResponse>>.Failure(
                    ErrorCodes.ValidationError,
                    "Validation failed",
                    400,
                    new Dictionary<string, string[]>
                    {
                        ["role"] = ["Role must be one of: student, lecturer, admin"]
                    });
            }

            normalizedRole = role;
        }

        var page = query.Page < 1 ? DefaultPage : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

        var accountQuery = new AccountQuery
        {
            Page = page,
            PageSize = pageSize,
            Role = normalizedRole,
            Keyword = string.IsNullOrWhiteSpace(query.Keyword) ? null : query.Keyword.Trim()
        };

        var accounts = await _accountRepository.ListAsync(accountQuery, cancellationToken);
        var response = new PagedResponse<AccountResponse>(
            accounts.Items.Select(MapAccount).ToArray(),
            page,
            pageSize,
            accounts.TotalItems);

        return ServiceResult<PagedResponse<AccountResponse>>.Success(response);
    }

    public async Task<ServiceResult<UserLockResponse>> LockUserAsync(
        Guid targetUserId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        if (targetUserId == currentAdminId)
        {
            return ServiceResult<UserLockResponse>.Failure(
                ErrorCodes.CannotLockSelf,
                "Admin cannot lock their own account",
                400);
        }

        var account = await _accountRepository.GetByIdAsync(targetUserId, cancellationToken);

        if (account is null)
        {
            return UserNotFound();
        }

        account.Lock(_clock.UtcNow);
        await _accountRepository.UpdateAsync(account, cancellationToken);

        return ServiceResult<UserLockResponse>.Success(new UserLockResponse
        {
            Id = account.Id,
            IsLocked = account.IsLocked
        });
    }

    public async Task<ServiceResult<UserLockResponse>> UnlockUserAsync(
        Guid targetUserId,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(targetUserId, cancellationToken);

        if (account is null)
        {
            return UserNotFound();
        }

        account.Unlock(_clock.UtcNow);
        await _accountRepository.UpdateAsync(account, cancellationToken);

        return ServiceResult<UserLockResponse>.Success(new UserLockResponse
        {
            Id = account.Id,
            IsLocked = account.IsLocked
        });
    }

    private static ServiceResult<UserLockResponse> UserNotFound()
    {
        return ServiceResult<UserLockResponse>.Failure(
            ErrorCodes.UserNotFound,
            "User not found",
            404);
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
