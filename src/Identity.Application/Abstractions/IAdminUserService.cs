using Identity.Application.Common;
using Identity.Application.Dtos;

namespace Identity.Application.Abstractions;

public interface IAdminUserService
{
    Task<ServiceResult<PagedResponse<AccountResponse>>> ListUsersAsync(
        AdminUsersQuery query,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserLockResponse>> LockUserAsync(
        Guid targetUserId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserLockResponse>> UnlockUserAsync(
        Guid targetUserId,
        CancellationToken cancellationToken = default);
}
