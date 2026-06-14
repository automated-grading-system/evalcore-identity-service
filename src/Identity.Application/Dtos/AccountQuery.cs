namespace Identity.Application.Dtos;

public sealed class AccountQuery
{
    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public string? Role { get; init; }

    public string? Keyword { get; init; }
}
