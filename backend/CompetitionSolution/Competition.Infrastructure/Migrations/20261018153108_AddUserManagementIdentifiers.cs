using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserManagementIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "AspNetUsers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValueSql: "replace(gen_random_uuid()::text, '-', '')");

            migrationBuilder.AddColumn<Instant>(
                name: "RegisteredAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PublicId",
                table: "AspNetUsers",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PublicId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RegisteredAt",
                table: "AspNetUsers");
        }
    }
}
