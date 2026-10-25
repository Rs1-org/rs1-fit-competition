using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchStatusOrderAndSelectionHomeStadium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HomeStadiumId",
                table: "Selections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "MatchStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Selections_HomeStadiumId",
                table: "Selections",
                column: "HomeStadiumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Selections_Stadiums_HomeStadiumId",
                table: "Selections",
                column: "HomeStadiumId",
                principalTable: "Stadiums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Selections_Stadiums_HomeStadiumId",
                table: "Selections");

            migrationBuilder.DropIndex(
                name: "IX_Selections_HomeStadiumId",
                table: "Selections");

            migrationBuilder.DropColumn(
                name: "HomeStadiumId",
                table: "Selections");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "MatchStatuses");
        }
    }
}
