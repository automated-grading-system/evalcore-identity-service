using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetAuditTimestamps();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<Account>(builder =>
        {
            builder.ToTable("accounts", "identity", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_accounts_role",
                    "role in ('student', 'lecturer', 'admin')");
            });

            builder.HasKey(account => account.Id)
                .HasName("pk_accounts");

            builder.Property(account => account.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .ValueGeneratedNever();

            builder.Property(account => account.FullName)
                .HasColumnName("full_name")
                .HasColumnType("varchar(120)")
                .HasMaxLength(120)
                .IsRequired();

            builder.Property(account => account.Email)
                .HasColumnName("email")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(account => account.PasswordHash)
                .HasColumnName("password_hash")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(account => account.Role)
                .HasColumnName("role")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(account => account.IsLocked)
                .HasColumnName("is_locked")
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(account => account.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(account => account.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.HasIndex(account => account.Email)
                .IsUnique()
                .HasDatabaseName("ux_accounts_email");

            builder.HasIndex(account => account.Role)
                .HasDatabaseName("ix_accounts_role");

            builder.HasIndex(account => account.IsLocked)
                .HasDatabaseName("ix_accounts_is_locked");
        });
    }

    private void SetAuditTimestamps()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Account>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(account => account.CreatedAt).CurrentValue =
                    entry.Property(account => account.CreatedAt).CurrentValue.ToUniversalTime();
                entry.Property(account => account.UpdatedAt).CurrentValue =
                    entry.Property(account => account.UpdatedAt).CurrentValue.ToUniversalTime();
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(account => account.UpdatedAt).CurrentValue = now;
            }
        }
    }
}
