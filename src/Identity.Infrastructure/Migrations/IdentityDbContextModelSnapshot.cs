using System;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Identity.Infrastructure.Migrations;

[DbContext(typeof(IdentityDbContext))]
partial class IdentityDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("identity")
            .HasAnnotation("ProductVersion", "8.0.0");

        modelBuilder.Entity("Identity.Domain.Entities.Account", builder =>
        {
            builder.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid")
                .HasColumnName("id");

            builder.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            builder.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("varchar(255)")
                .HasColumnName("email");

            builder.Property<string>("FullName")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("varchar(120)")
                .HasColumnName("full_name");

            builder.Property<bool>("IsLocked")
                .ValueGeneratedOnAdd()
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .HasColumnName("is_locked");

            builder.Property<string>("PasswordHash")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("password_hash");

            builder.Property<string>("Role")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("role");

            builder.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");

            builder.HasKey("Id")
                .HasName("pk_accounts");

            builder.HasIndex("IsLocked")
                .HasDatabaseName("ix_accounts_is_locked");

            builder.HasIndex("Role")
                .HasDatabaseName("ix_accounts_role");

            builder.HasIndex("Email")
                .IsUnique()
                .HasDatabaseName("ux_accounts_email");

            builder.ToTable("accounts", "identity", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_accounts_role",
                    "role in ('student', 'lecturer', 'admin')");
            });
        });
#pragma warning restore 612, 618
    }
}
