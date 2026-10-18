using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Statistics_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StatisticId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalShots = table.Column<int>(type: "integer", nullable: false),
                    ShotsOnTarget = table.Column<int>(type: "integer", nullable: false),
                    TotalRedCards = table.Column<int>(type: "integer", nullable: false),
                    TotalYellowCards = table.Column<int>(type: "integer", nullable: false),
                    Corners = table.Column<int>(type: "integer", nullable: false),
                    BigChances = table.Column<int>(type: "integer", nullable: false),
                    Possession = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamStatistics", x => x.Id);
                    table.CheckConstraint("CK_TeamStatistics_NonNegative", "\"TotalShots\" >= 0 AND \"ShotsOnTarget\" >= 0 AND \"TotalRedCards\" >= 0 AND \"TotalYellowCards\" >= 0 AND \"Corners\" >= 0 AND \"BigChances\" >= 0");
                    table.CheckConstraint("CK_TeamStatistics_Possession", "\"Possession\" >= 0 AND \"Possession\" <= 100");
                    table.CheckConstraint("CK_TeamStatistics_ShotsOnTarget", "\"ShotsOnTarget\" <= \"TotalShots\"");
                    table.ForeignKey(
                        name: "FK_TeamStatistics_Selections_SelectionId",
                        column: x => x.SelectionId,
                        principalTable: "Selections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamStatistics_Statistics_StatisticId",
                        column: x => x.StatisticId,
                        principalTable: "Statistics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_MatchId",
                table: "Statistics",
                column: "MatchId",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_PublicId",
                table: "Statistics",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_PublicId",
                table: "TeamStatistics",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_SelectionId",
                table: "TeamStatistics",
                column: "SelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_StatisticId_SelectionId",
                table: "TeamStatistics",
                columns: new[] { "StatisticId", "SelectionId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamStatistics");

            migrationBuilder.DropTable(
                name: "Statistics");
        }
    }
}
