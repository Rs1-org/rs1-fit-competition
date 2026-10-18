using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchParts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchPartId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    AddedMinute = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.CheckConstraint("CK_Events_DifferentPlayers", "\"PlayerId\" IS NULL OR \"RelatedPlayerId\" IS NULL OR \"PlayerId\" <> \"RelatedPlayerId\"");
                    table.CheckConstraint("CK_Events_Minute", "\"Minute\" BETWEEN 0 AND 130 AND \"AddedMinute\" BETWEEN 0 AND 30");
                    table.ForeignKey(
                        name: "FK_Events_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_MatchParts_MatchPartId",
                        column: x => x.MatchPartId,
                        principalTable: "MatchParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Players_RelatedPlayerId",
                        column: x => x.RelatedPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Selections_SelectionId",
                        column: x => x.SelectionId,
                        principalTable: "Selections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeId",
                table: "Events",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_MatchId_MatchPartId_Minute_AddedMinute",
                table: "Events",
                columns: new[] { "MatchId", "MatchPartId", "Minute", "AddedMinute" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_MatchPartId",
                table: "Events",
                column: "MatchPartId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_PlayerId",
                table: "Events",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_PublicId",
                table: "Events",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_RelatedPlayerId",
                table: "Events",
                column: "RelatedPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SelectionId",
                table: "Events",
                column: "SelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_Code",
                table: "EventTypes",
                column: "Code",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_PublicId",
                table: "EventTypes",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchParts_Code",
                table: "MatchParts",
                column: "Code",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MatchParts_PublicId",
                table: "MatchParts",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "MatchParts");
        }
    }
}
