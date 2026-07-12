using Identity.Application.Abstractions;
using Identity.Application.Common;
using Identity.Application.Dtos;
using Identity.Application.Errors;

namespace Identity.Application.Services;

public sealed class InternalUserService : IInternalUserService
{
    private readonly IAccountRepository _accountRepository;

    public InternalUserService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<ServiceResult<InternalUserResponse>> GetInternalUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(userId, cancellationToken);
        if (account is null)
        {
            return ServiceResult<InternalUserResponse>.Failure(
                ErrorCodes.UserNotFound,
                "User not found",
                404);
        }

        return ServiceResult<InternalUserResponse>.Success(new InternalUserResponse
        {
            Id = account.Id,
            Email = account.Email,
            FullName = account.FullName,
            Role = account.Role
        });
    }
}
