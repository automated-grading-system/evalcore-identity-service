using Identity.Application.Common;
using Identity.Application.Dtos;
using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<Account?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Account>> ListAsync(AccountQuery query, CancellationToken cancellationToken = default);

    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
}
