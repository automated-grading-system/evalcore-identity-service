using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    private const string LocalConnectionString =
        "Host=localhost;Port=5432;Database=ags;Username=ags;Password=ags_password";

    public IdentityDbContext CreateDbContext(string[] args)
    {
        DesignTimeEnvFileLoader.LoadFromNearest(
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(ResolveRepositoryRoot())
            .AddJsonFile("src/Identity.Api/appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("src/Identity.Api/appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration["DATABASE_URL"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? LocalConnectionString;

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new IdentityDbContext(options);
    }

    private static string ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "IdentityService.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
