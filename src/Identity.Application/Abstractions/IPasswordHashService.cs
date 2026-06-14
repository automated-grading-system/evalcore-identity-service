using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

public interface IPasswordHashService
{
    string HashPassword(Account account, string password);

    bool VerifyPassword(Account account, string password);
}
