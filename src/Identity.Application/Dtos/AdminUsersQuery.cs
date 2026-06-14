namespace Identity.Application.Dtos;

public sealed class AdminUsersQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Role { get; set; }

    public string? Keyword { get; set; }
}
