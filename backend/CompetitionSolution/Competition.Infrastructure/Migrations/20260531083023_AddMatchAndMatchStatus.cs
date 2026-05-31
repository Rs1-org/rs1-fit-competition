using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchAndMatchStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayScore = table.Column<int>(type: "integer", nullable: true),
                    HalfHomeScore = table.Column<int>(type: "integer", nullable: true),
                    HalfAwayScore = table.Column<int>(type: "integer", nullable: true),
                    IsPostponed = table.Column<bool>(type: "boolean", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    StadiumId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_MatchStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "MatchStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Stadiums_StadiumId",
                        column: x => x.StadiumId,
                        principalTable: "Stadiums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Slug",
                table: "Matches",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_StadiumId",
                table: "Matches",
                column: "StadiumId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_StatusId",
                table: "Matches",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchStatuses_Slug",
                table: "MatchStatuses",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "MatchStatuses");
        }
    }
}
