using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<LocalTime>(type: "time", nullable: false),
                    Picture = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO "Persons" ("Id", "FirstName", "LastName", "DateOfBirth", "Picture", "CreatedAt", "ModifiedAt", "DeletedAt")
                SELECT "Id", '', '', '00:00:00'::time, '\x'::bytea, "CreatedAt", "ModifiedAt", "DeletedAt"
                FROM "Players";
                """);

            migrationBuilder.Sql("""
                INSERT INTO "Persons" ("Id", "FirstName", "LastName", "DateOfBirth", "Picture", "CreatedAt", "ModifiedAt", "DeletedAt")
                SELECT "Id", '', '', '00:00:00'::time, '\x'::bytea, "CreatedAt", "ModifiedAt", "DeletedAt"
                FROM "Managers";
                """);

            migrationBuilder.Sql("""
                INSERT INTO "Persons" ("Id", "FirstName", "LastName", "DateOfBirth", "Picture", "CreatedAt", "ModifiedAt", "DeletedAt")
                SELECT "Id", '', '', '00:00:00'::time, '\x'::bytea, "CreatedAt", "ModifiedAt", "DeletedAt"
                FROM "Referees";
                """);

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Referees");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Referees");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Referees");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Managers");

            migrationBuilder.Sql("""
                ALTER TABLE "Referees" ALTER COLUMN "CareerStart" DROP NOT NULL;
                ALTER TABLE "Referees" ALTER COLUMN "CareerStart" TYPE time USING "CareerStart"::time;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Managers" ALTER COLUMN "CareerStart" DROP NOT NULL;
                ALTER TABLE "Managers" ALTER COLUMN "CareerStart" TYPE time USING "CareerStart"::time;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Persons_Id",
                table: "Managers",
                column: "Id",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Persons_Id",
                table: "Players",
                column: "Id",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referees_Persons_Id",
                table: "Referees",
                column: "Id",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Persons_Id",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Persons_Id",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Referees_Persons_Id",
                table: "Referees");

            migrationBuilder.AddColumn<Instant>(
                name: "CreatedAt",
                table: "Referees",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "DeletedAt",
                table: "Referees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "ModifiedAt",
                table: "Referees",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "CreatedAt",
                table: "Players",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "DeletedAt",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "ModifiedAt",
                table: "Players",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "CreatedAt",
                table: "Managers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "DeletedAt",
                table: "Managers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "ModifiedAt",
                table: "Managers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.Sql("""
                UPDATE "Referees" r
                SET "CreatedAt" = p."CreatedAt", "ModifiedAt" = p."ModifiedAt", "DeletedAt" = p."DeletedAt"
                FROM "Persons" p WHERE r."Id" = p."Id";
                """);

            migrationBuilder.Sql("""
                UPDATE "Players" pl
                SET "CreatedAt" = p."CreatedAt", "ModifiedAt" = p."ModifiedAt", "DeletedAt" = p."DeletedAt"
                FROM "Persons" p WHERE pl."Id" = p."Id";
                """);

            migrationBuilder.Sql("""
                UPDATE "Managers" m
                SET "CreatedAt" = p."CreatedAt", "ModifiedAt" = p."ModifiedAt", "DeletedAt" = p."DeletedAt"
                FROM "Persons" p WHERE m."Id" = p."Id";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Referees" ALTER COLUMN "CareerStart" TYPE timestamp with time zone
                USING ('1970-01-01'::date + "CareerStart") AT TIME ZONE 'UTC';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Managers" ALTER COLUMN "CareerStart" TYPE timestamp with time zone
                USING ('1970-01-01'::date + "CareerStart") AT TIME ZONE 'UTC';
                """);

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
