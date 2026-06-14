using Identity.Application.Common;
using Identity.Application.Dtos;

namespace Identity.Application.Abstractions;

public interface IUserService
{
    Task<ServiceResult<UserProfileResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
