using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePersonFieldsToLocalDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Persons" ALTER COLUMN "DateOfBirth" TYPE date USING DATE '2000-01-01';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Managers" ALTER COLUMN "CareerStart" TYPE date USING NULL;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Referees" ALTER COLUMN "CareerStart" TYPE date USING NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Persons" ALTER COLUMN "DateOfBirth" TYPE time USING '00:00:00'::time;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Managers" ALTER COLUMN "CareerStart" TYPE time USING NULL;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Referees" ALTER COLUMN "CareerStart" TYPE time USING NULL;
                """);
        }
    }
}
