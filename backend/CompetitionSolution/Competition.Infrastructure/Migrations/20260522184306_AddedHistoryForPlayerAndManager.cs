using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedHistoryForPlayerAndManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerSelectionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Goals = table.Column<int>(type: "integer", nullable: true),
                    MatchPlayed = table.Column<int>(type: "integer", nullable: true),
                    Assists = table.Column<int>(type: "integer", nullable: true),
                    ShirtNumber = table.Column<int>(type: "integer", nullable: true),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSelectionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSelectionHistories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelectionManagerHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Wins = table.Column<int>(type: "integer", nullable: true),
                    Loses = table.Column<int>(type: "integer", nullable: true),
                    Draw = table.Column<int>(type: "integer", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectionManagerHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelectionManagerHistories_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferValueHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Date = table.Column<LocalDate>(type: "date", nullable: true),
                    Value = table.Column<decimal>(type: "numeric", nullable: true),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferValueHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferValueHistories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MandatDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StartMandatDate = table.Column<LocalDate>(type: "date", nullable: true),
                    EndMandatDate = table.Column<LocalDate>(type: "date", nullable: true),
                    PlayerSelectionHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectionManagerHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandatDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MandatDates_PlayerSelectionHistories_PlayerSelectionHistory~",
                        column: x => x.PlayerSelectionHistoryId,
                        principalTable: "PlayerSelectionHistories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MandatDates_SelectionManagerHistories_SelectionManagerHisto~",
                        column: x => x.SelectionManagerHistoryId,
                        principalTable: "SelectionManagerHistories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MandatDates_PlayerSelectionHistoryId",
                table: "MandatDates",
                column: "PlayerSelectionHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MandatDates_SelectionManagerHistoryId",
                table: "MandatDates",
                column: "SelectionManagerHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSelectionHistories_PlayerId",
                table: "PlayerSelectionHistories",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectionManagerHistories_ManagerId",
                table: "SelectionManagerHistories",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferValueHistories_PlayerId",
                table: "TransferValueHistories",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MandatDates");

            migrationBuilder.DropTable(
                name: "TransferValueHistories");

            migrationBuilder.DropTable(
                name: "PlayerSelectionHistories");

            migrationBuilder.DropTable(
                name: "SelectionManagerHistories");
        }
    }
}
