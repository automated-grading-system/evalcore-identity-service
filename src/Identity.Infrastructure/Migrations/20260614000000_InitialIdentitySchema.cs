using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations;

public partial class InitialIdentitySchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "identity");

        migrationBuilder.CreateTable(
            name: "accounts",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                full_name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                role = table.Column<string>(type: "text", nullable: false),
                is_locked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_accounts", account => account.id);
                table.CheckConstraint("ck_accounts_role", "role in ('student', 'lecturer', 'admin')");
            });

        migrationBuilder.CreateIndex(
            name: "ix_accounts_is_locked",
            schema: "identity",
            table: "accounts",
            column: "is_locked");

        migrationBuilder.CreateIndex(
            name: "ix_accounts_role",
            schema: "identity",
            table: "accounts",
            column: "role");

        migrationBuilder.CreateIndex(
            name: "ux_accounts_email",
            schema: "identity",
            table: "accounts",
            column: "email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "accounts",
            schema: "identity");
    }
}
