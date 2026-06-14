using Identity.Application.Common;
using Identity.Application.Dtos;

namespace Identity.Application.Abstractions;

public interface IAuthService
{
    Task<ServiceResult<AccountResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
