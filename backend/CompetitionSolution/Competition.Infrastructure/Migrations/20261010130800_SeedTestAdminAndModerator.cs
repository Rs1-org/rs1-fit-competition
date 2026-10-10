using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
//admin@fit.local pass: Admin123!
//moderator@fit.local pass: Moderator123!
namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestAdminAndModerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
                SELECT 'a1111111-1111-1111-1111-111111111111', 'Admin', 'ADMIN', 'seed-admin-role'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMIN'
                );

                INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
                SELECT 'a2222222-2222-2222-2222-222222222222', 'Moderator', 'MODERATOR', 'seed-moderator-role'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'MODERATOR'
                );

                INSERT INTO "AspNetUsers" (
                    "Id", "FirstName", "LastName", "IsActive",
                    "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount")
                SELECT
                    'b1111111-1111-1111-1111-111111111111',
                    'Test', 'Admin', TRUE,
                    'admin@fit.local', 'ADMIN@FIT.LOCAL', 'admin@fit.local', 'ADMIN@FIT.LOCAL',
                    TRUE,
                    'AQAAAAIAAYagAAAAEFMV98tOqinb6+aZ4euyZT9LY/p40bx8Tu1qC7qWU0N5NhPYn51B28pZbkn/Yj6gFw==',
                    'c1111111-1111-1111-1111-111111111111',
                    'd1111111-1111-1111-1111-111111111111',
                    FALSE, FALSE, TRUE, 0
                WHERE NOT EXISTS (
                    SELECT 1 FROM "AspNetUsers" WHERE "NormalizedUserName" = 'ADMIN@FIT.LOCAL'
                );

                INSERT INTO "AspNetUsers" (
                    "Id", "FirstName", "LastName", "IsActive",
                    "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount")
                SELECT
                    'b2222222-2222-2222-2222-222222222222',
                    'Test', 'Moderator', TRUE,
                    'moderator@fit.local', 'MODERATOR@FIT.LOCAL', 'moderator@fit.local', 'MODERATOR@FIT.LOCAL',
                    TRUE,
                    'AQAAAAIAAYagAAAAEKOASaeH5BjFk28mJA/3/E894drLqlvrnbxckWXtOIe6iXLbRH+LiYSSnlPMWwzOUQ==',
                    'c2222222-2222-2222-2222-222222222222',
                    'd2222222-2222-2222-2222-222222222222',
                    FALSE, FALSE, TRUE, 0
                WHERE NOT EXISTS (
                    SELECT 1 FROM "AspNetUsers" WHERE "NormalizedUserName" = 'MODERATOR@FIT.LOCAL'
                );

                INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
                SELECT u."Id", r."Id"
                FROM "AspNetUsers" u
                JOIN "AspNetRoles" r ON r."NormalizedName" = 'ADMIN'
                WHERE u."NormalizedUserName" = 'ADMIN@FIT.LOCAL'
                  AND NOT EXISTS (
                      SELECT 1 FROM "AspNetUserRoles" ur
                      WHERE ur."UserId" = u."Id" AND ur."RoleId" = r."Id"
                  );

                INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
                SELECT u."Id", r."Id"
                FROM "AspNetUsers" u
                JOIN "AspNetRoles" r ON r."NormalizedName" = 'MODERATOR'
                WHERE u."NormalizedUserName" = 'MODERATOR@FIT.LOCAL'
                  AND NOT EXISTS (
                      SELECT 1 FROM "AspNetUserRoles" ur
                      WHERE ur."UserId" = u."Id" AND ur."RoleId" = r."Id"
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "AspNetUserRoles" ur
                USING "AspNetUsers" u
                WHERE ur."UserId" = u."Id"
                  AND u."NormalizedUserName" IN ('ADMIN@FIT.LOCAL', 'MODERATOR@FIT.LOCAL');

                DELETE FROM "AspNetUsers"
                WHERE "NormalizedUserName" IN ('ADMIN@FIT.LOCAL', 'MODERATOR@FIT.LOCAL');
                """);
        }
    }
}
