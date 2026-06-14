namespace Identity.Infrastructure.Persistence;

internal static class DesignTimeEnvFileLoader
{
    public static void LoadFromNearest(params string[] startDirectories)
    {
        foreach (var startDirectory in startDirectories)
        {
            var envFilePath = FindNearest(startDirectory, ".env");

            if (envFilePath is not null)
            {
                LoadIfExists(envFilePath);
                return;
            }
        }
    }

    private static void LoadIfExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(filePath))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"').Trim('\'');

            if (!string.IsNullOrWhiteSpace(key) && Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? FindNearest(string? startDirectory, string fileName)
    {
        if (string.IsNullOrWhiteSpace(startDirectory))
        {
            return null;
        }

        var directory = new DirectoryInfo(startDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, fileName);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
