using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureGeoEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old rows may have NULL or too-long values; fix them before NOT NULL / max length apply.
            foreach (var (table, nameMax, slugMax) in new[] { ("Countries", 100, 120), ("Cities", 100, 120), ("Stadiums", 150, 170) })
            {
                migrationBuilder.Sql($"UPDATE \"{table}\" SET \"Name\" = 'Unnamed' WHERE \"Name\" IS NULL OR \"Name\" = '';");
                migrationBuilder.Sql($"UPDATE \"{table}\" SET \"Name\" = left(\"Name\", {nameMax}) WHERE length(\"Name\") > {nameMax};");
                migrationBuilder.Sql($"UPDATE \"{table}\" SET \"Slug\" = 'item-' || left(replace(\"Id\"::text, '-', ''), 12) WHERE \"Slug\" IS NULL OR \"Slug\" = '';");
                migrationBuilder.Sql($"UPDATE \"{table}\" SET \"Slug\" = left(\"Slug\", {slugMax}) WHERE length(\"Slug\") > {slugMax};");
            }
            migrationBuilder.Sql("UPDATE \"Countries\" SET \"PictureUrl\" = NULL WHERE length(\"PictureUrl\") > 500;");
            migrationBuilder.Sql("UPDATE \"Stadiums\" SET \"PictureUrl\" = NULL WHERE length(\"PictureUrl\") > 500;");
            migrationBuilder.Sql("UPDATE \"GeoDatas\" SET \"Description\" = left(\"Description\", 1000) WHERE length(\"Description\") > 1000;");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_GeoDatas_GeoDataId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Stadiums_Cities_CityId",
                table: "Stadiums");

            migrationBuilder.DropForeignKey(
                name: "FK_Stadiums_GeoDatas_GeoDataId",
                table: "Stadiums");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Stadiums",
                type: "character varying(170)",
                maxLength: 170,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PictureUrl",
                table: "Stadiums",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Stadiums",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "GeoDatas",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Countries",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PictureUrl",
                table: "Countries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Countries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Cities",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_GeoDatas_GeoDataId",
                table: "Cities",
                column: "GeoDataId",
                principalTable: "GeoDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stadiums_Cities_CityId",
                table: "Stadiums",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stadiums_GeoDatas_GeoDataId",
                table: "Stadiums",
                column: "GeoDataId",
                principalTable: "GeoDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_GeoDatas_GeoDataId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Stadiums_Cities_CityId",
                table: "Stadiums");

            migrationBuilder.DropForeignKey(
                name: "FK_Stadiums_GeoDatas_GeoDataId",
                table: "Stadiums");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Stadiums",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(170)",
                oldMaxLength: 170);

            migrationBuilder.AlterColumn<string>(
                name: "PictureUrl",
                table: "Stadiums",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Stadiums",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "GeoDatas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Countries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "PictureUrl",
                table: "Countries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Countries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Cities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_GeoDatas_GeoDataId",
                table: "Cities",
                column: "GeoDataId",
                principalTable: "GeoDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stadiums_Cities_CityId",
                table: "Stadiums",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stadiums_GeoDatas_GeoDataId",
                table: "Stadiums",
                column: "GeoDataId",
                principalTable: "GeoDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
