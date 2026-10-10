using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkPlayerSelectionAndMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerSelectionHistories_Players_PlayerId",
                table: "PlayerSelectionHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SelectionManagerHistories_Managers_ManagerId",
                table: "SelectionManagerHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "SelectionId",
                table: "SelectionManagerHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectionId",
                table: "PlayerSelectionHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AwaySelectionId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HomeSelectionId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Selections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PictureUrl = table.Column<string>(type: "text", nullable: true),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Selections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Selections_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SelectionManagerHistories_SelectionId",
                table: "SelectionManagerHistories",
                column: "SelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSelectionHistories_SelectionId",
                table: "PlayerSelectionHistories",
                column: "SelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AwaySelectionId",
                table: "Matches",
                column: "AwaySelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_HomeSelectionId",
                table: "Matches",
                column: "HomeSelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Selections_CountryId",
                table: "Selections",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Selections_Slug",
                table: "Selections",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Selections_AwaySelectionId",
                table: "Matches",
                column: "AwaySelectionId",
                principalTable: "Selections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Selections_HomeSelectionId",
                table: "Matches",
                column: "HomeSelectionId",
                principalTable: "Selections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerSelectionHistories_Players_PlayerId",
                table: "PlayerSelectionHistories",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerSelectionHistories_Selections_SelectionId",
                table: "PlayerSelectionHistories",
                column: "SelectionId",
                principalTable: "Selections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SelectionManagerHistories_Managers_ManagerId",
                table: "SelectionManagerHistories",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SelectionManagerHistories_Selections_SelectionId",
                table: "SelectionManagerHistories",
                column: "SelectionId",
                principalTable: "Selections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Selections_AwaySelectionId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Selections_HomeSelectionId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerSelectionHistories_Players_PlayerId",
                table: "PlayerSelectionHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerSelectionHistories_Selections_SelectionId",
                table: "PlayerSelectionHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SelectionManagerHistories_Managers_ManagerId",
                table: "SelectionManagerHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SelectionManagerHistories_Selections_SelectionId",
                table: "SelectionManagerHistories");

            migrationBuilder.DropTable(
                name: "Selections");

            migrationBuilder.DropIndex(
                name: "IX_SelectionManagerHistories_SelectionId",
                table: "SelectionManagerHistories");

            migrationBuilder.DropIndex(
                name: "IX_PlayerSelectionHistories_SelectionId",
                table: "PlayerSelectionHistories");

            migrationBuilder.DropIndex(
                name: "IX_Matches_AwaySelectionId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_HomeSelectionId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SelectionId",
                table: "SelectionManagerHistories");

            migrationBuilder.DropColumn(
                name: "SelectionId",
                table: "PlayerSelectionHistories");

            migrationBuilder.DropColumn(
                name: "AwaySelectionId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HomeSelectionId",
                table: "Matches");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerSelectionHistories_Players_PlayerId",
                table: "PlayerSelectionHistories",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SelectionManagerHistories_Managers_ManagerId",
                table: "SelectionManagerHistories",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
