namespace Identity.Domain.Constants;

public static class AccountRoles
{
    public const string Student = "student";
    public const string Lecturer = "lecturer";
    public const string Admin = "admin";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Student,
        Lecturer,
        Admin
    };

    public static bool IsValid(string? role)
    {
        return !string.IsNullOrWhiteSpace(role) && All.Contains(role.Trim());
    }

    public static bool TryNormalize(string? role, out string normalizedRole)
    {
        normalizedRole = string.Empty;

        if (!IsValid(role))
        {
            return false;
        }

        normalizedRole = role!.Trim().ToLowerInvariant();
        return true;
    }
}
