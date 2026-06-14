using Identity.Application.Abstractions;
using Identity.Application.Common;
using Identity.Application.Dtos;
using Identity.Application.Errors;

namespace Identity.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IAccountRepository _accountRepository;

    public UserService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<ServiceResult<UserProfileResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(userId, cancellationToken);

        if (account is null)
        {
            return ServiceResult<UserProfileResponse>.Failure(
                ErrorCodes.UserNotFound,
                "User not found",
                404);
        }

        return ServiceResult<UserProfileResponse>.Success(new UserProfileResponse
        {
            Id = account.Id,
            FullName = account.FullName,
            Email = account.Email,
            Role = account.Role
        });
    }
}
