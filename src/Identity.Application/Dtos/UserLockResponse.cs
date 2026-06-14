namespace Identity.Application.Dtos;

public sealed class UserLockResponse
{
    public required Guid Id { get; init; }

    public required bool IsLocked { get; init; }
}
