namespace Identity.Application.Dtos;

public sealed class AccountResponse
{
    public required Guid Id { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public required string Role { get; init; }

    public required bool IsLocked { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}
