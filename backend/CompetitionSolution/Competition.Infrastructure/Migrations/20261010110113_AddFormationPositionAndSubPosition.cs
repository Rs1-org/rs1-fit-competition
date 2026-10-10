using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFormationPositionAndSubPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "Players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubPositionId",
                table: "Players",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormationMarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Defenders = table.Column<int>(type: "integer", nullable: false),
                    Midfielders = table.Column<int>(type: "integer", nullable: false),
                    Attackers = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormationMarks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Formations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    FormationMarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Formations_FormationMarks_FormationMarkId",
                        column: x => x.FormationMarkId,
                        principalTable: "FormationMarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubPositions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_PositionId",
                table: "Players",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_SubPositionId",
                table: "Players",
                column: "SubPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormationMarks_Slug",
                table: "FormationMarks",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Formations_FormationMarkId",
                table: "Formations",
                column: "FormationMarkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Formations_Slug",
                table: "Formations",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Slug",
                table: "Positions",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubPositions_PositionId",
                table: "SubPositions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubPositions_Slug",
                table: "SubPositions",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Positions_PositionId",
                table: "Players",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_SubPositions_SubPositionId",
                table: "Players",
                column: "SubPositionId",
                principalTable: "SubPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Positions_PositionId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_SubPositions_SubPositionId",
                table: "Players");

            migrationBuilder.DropTable(
                name: "Formations");

            migrationBuilder.DropTable(
                name: "SubPositions");

            migrationBuilder.DropTable(
                name: "FormationMarks");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Players_PositionId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_SubPositionId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "SubPositionId",
                table: "Players");
        }
    }
}
