using Identity.Application.Common;
using Identity.Application.Dtos;

namespace Identity.Application.Abstractions;

public interface IInternalUserService
{
    Task<ServiceResult<InternalUserResponse>> GetInternalUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
