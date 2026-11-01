using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchLineupsAndReferee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AwayFormationId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HomeFormationId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RefereeId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatchLineups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsStarter = table.Column<bool>(type: "boolean", nullable: false),
                    ShirtNumber = table.Column<int>(type: "integer", nullable: true),
                    PublicId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchLineups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchLineups_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchLineups_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchLineups_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchLineups_Selections_SelectionId",
                        column: x => x.SelectionId,
                        principalTable: "Selections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchLineups_SubPositions_SubPositionId",
                        column: x => x.SubPositionId,
                        principalTable: "SubPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AwayFormationId",
                table: "Matches",
                column: "AwayFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_HomeFormationId",
                table: "Matches",
                column: "HomeFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RefereeId",
                table: "Matches",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineups_MatchId_PlayerId",
                table: "MatchLineups",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineups_MatchId_SelectionId_ShirtNumber",
                table: "MatchLineups",
                columns: new[] { "MatchId", "SelectionId", "ShirtNumber" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL AND \"ShirtNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineups_PlayerId",
                table: "MatchLineups",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineups_PositionId",
                table: "MatchLineups",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineups_PublicId",
                table: "MatchLineups",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineups_SelectionId",
                table: "MatchLineups",
                column: "SelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineups_SubPositionId",
                table: "MatchLineups",
                column: "SubPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Formations_AwayFormationId",
                table: "Matches",
                column: "AwayFormationId",
                principalTable: "Formations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Formations_HomeFormationId",
                table: "Matches",
                column: "HomeFormationId",
                principalTable: "Formations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Referees_RefereeId",
                table: "Matches",
                column: "RefereeId",
                principalTable: "Referees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Formations_AwayFormationId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Formations_HomeFormationId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Referees_RefereeId",
                table: "Matches");

            migrationBuilder.DropTable(
                name: "MatchLineups");

            migrationBuilder.DropIndex(
                name: "IX_Matches_AwayFormationId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_HomeFormationId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_RefereeId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "AwayFormationId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HomeFormationId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "RefereeId",
                table: "Matches");
        }
    }
}
