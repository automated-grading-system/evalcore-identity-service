using Identity.Application.Abstractions;
using Identity.Application.Common;
using Identity.Application.Dtos;
using Identity.Application.Exceptions;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Identity.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private const string UniqueViolationSqlState = "23505";

    private readonly IdentityDbContext _dbContext;

    public AccountRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        _dbContext.Accounts.Add(account);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueEmailViolation(exception))
        {
            throw new DuplicateEmailException(account.Email);
        }
    }

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return _dbContext.Accounts.AnyAsync(
            account => account.Email == normalizedEmail,
            cancellationToken);
    }

    public Task<Account?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Email == normalizedEmail, cancellationToken);
    }

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Accounts.FirstOrDefaultAsync(account => account.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Account>> ListAsync(
        AccountQuery query,
        CancellationToken cancellationToken = default)
    {
        var accounts = _dbContext.Accounts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            accounts = accounts.Where(account => account.Role == query.Role);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLowerInvariant();
            accounts = accounts.Where(account =>
                account.FullName.ToLower().Contains(keyword)
                || account.Email.ToLower().Contains(keyword));
        }

        var totalItems = await accounts.CountAsync(cancellationToken);

        var items = await accounts
            .OrderByDescending(account => account.CreatedAt)
            .ThenBy(account => account.Email)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Account>(items, totalItems);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _dbContext.Accounts.Update(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUniqueEmailViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == UniqueViolationSqlState
            && postgresException.ConstraintName == "ux_accounts_email";
    }
}
