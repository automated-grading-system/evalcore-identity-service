using Identity.Domain.Constants;

namespace Identity.Domain.Entities;

public sealed class Account
{
    private Account()
    {
    }

    private Account(
        Guid id,
        string fullName,
        string email,
        string passwordHash,
        string role,
        bool isLocked,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsLocked = isLocked;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Role { get; private set; } = AccountRoles.Student;

    public bool IsLocked { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Account Create(
        string fullName,
        string email,
        string role,
        DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (fullName.Trim().Length > 120)
        {
            throw new ArgumentException("Full name cannot exceed 120 characters.", nameof(fullName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (!AccountRoles.TryNormalize(role, out var normalizedRole))
        {
            throw new ArgumentException("Role is invalid.", nameof(role));
        }

        var timestamp = createdAtUtc.ToUniversalTime();

        return new Account(
            Guid.NewGuid(),
            fullName.Trim(),
            NormalizeEmail(email),
            string.Empty,
            normalizedRole,
            isLocked: false,
            timestamp,
            timestamp);
    }

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public void SetPasswordHash(string passwordHash, DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        PasswordHash = passwordHash;
        Touch(updatedAtUtc);
    }

    public void Lock(DateTimeOffset updatedAtUtc)
    {
        IsLocked = true;
        Touch(updatedAtUtc);
    }

    public void Unlock(DateTimeOffset updatedAtUtc)
    {
        IsLocked = false;
        Touch(updatedAtUtc);
    }

    private void Touch(DateTimeOffset updatedAtUtc)
    {
        UpdatedAt = updatedAtUtc.ToUniversalTime();
    }
}
