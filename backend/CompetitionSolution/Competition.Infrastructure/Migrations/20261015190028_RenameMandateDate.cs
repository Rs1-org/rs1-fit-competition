using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameMandateDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransferValueHistories_Players_PlayerId",
                table: "TransferValueHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatDates_PlayerSelectionHistories_PlayerSelectionHistory~",
                table: "MandatDates");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatDates_SelectionManagerHistories_SelectionManagerHisto~",
                table: "MandatDates");

            migrationBuilder.RenameTable(
                name: "MandatDates",
                newName: "MandateDates");

            migrationBuilder.RenameColumn(
                name: "StartMandatDate",
                table: "MandateDates",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndMandatDate",
                table: "MandateDates",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_MandatDates_PlayerSelectionHistoryId",
                table: "MandateDates",
                newName: "IX_MandateDates_PlayerSelectionHistoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MandatDates_SelectionManagerHistoryId",
                table: "MandateDates",
                newName: "IX_MandateDates_SelectionManagerHistoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MandatDates_PublicId",
                table: "MandateDates",
                newName: "IX_MandateDates_PublicId");

            migrationBuilder.Sql("""ALTER TABLE "MandateDates" RENAME CONSTRAINT "PK_MandatDates" TO "PK_MandateDates";""");

            migrationBuilder.AddForeignKey(
                name: "FK_MandateDates_PlayerSelectionHistories_PlayerSelectionHistor~",
                table: "MandateDates",
                column: "PlayerSelectionHistoryId",
                principalTable: "PlayerSelectionHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MandateDates_SelectionManagerHistories_SelectionManagerHist~",
                table: "MandateDates",
                column: "SelectionManagerHistoryId",
                principalTable: "SelectionManagerHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "TransferValueHistories",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "SelectionManagerHistories",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferValueHistories_Players_PlayerId",
                table: "TransferValueHistories",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransferValueHistories_Players_PlayerId",
                table: "TransferValueHistories");

            migrationBuilder.DropTable(
                name: "MandateDates");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "TransferValueHistories",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "SelectionManagerHistories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "MandatDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PlayerSelectionHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectionManagerHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    EndMandatDate = table.Column<LocalDate>(type: "date", nullable: true),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartMandatDate = table.Column<LocalDate>(type: "date", nullable: true)
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
                name: "IX_MandatDates_PublicId",
                table: "MandatDates",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MandatDates_SelectionManagerHistoryId",
                table: "MandatDates",
                column: "SelectionManagerHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferValueHistories_Players_PlayerId",
                table: "TransferValueHistories",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
