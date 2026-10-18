using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkSelectionToClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClubId",
                table: "Selections",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Selections_ClubId",
                table: "Selections",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Selections_Clubs_ClubId",
                table: "Selections",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Selections_Clubs_ClubId",
                table: "Selections");

            migrationBuilder.DropIndex(
                name: "IX_Selections_ClubId",
                table: "Selections");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "Selections");
        }
    }
}
