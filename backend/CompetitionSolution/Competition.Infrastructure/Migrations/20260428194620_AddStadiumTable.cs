using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStadiumTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stadiums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    DateOfConstruction = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    GeoDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    PictureUrl = table.Column<string>(type: "text", nullable: true),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stadiums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stadiums_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stadiums_GeoDatas_GeoDataId",
                        column: x => x.GeoDataId,
                        principalTable: "GeoDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stadiums_CityId",
                table: "Stadiums",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Stadiums_GeoDataId",
                table: "Stadiums",
                column: "GeoDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stadiums");
        }
    }
}
