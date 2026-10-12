using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Competition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdToBaseEntity : Migration
    {
        // Every root BaseEntity table (Player/Manager/Referee share Persons).
        private static readonly string[] Tables =
        {
            "Cities", "Competitions", "Countries", "FormationMarks", "Formations", "GeoDatas", "MandatDates", "MatchStatuses", "Matches", "Persons", "PlayerSelectionHistories", "Positions", "Rounds", "Seasons", "SelectionManagerHistories", "Selections", "Stadiums", "SubPositions", "TransferValueHistories"
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                migrationBuilder.AddColumn<string>(
                    name: "PublicId",
                    table: table,
                    type: "character varying(32)",
                    maxLength: 32,
                    nullable: true);

                // Existing rows get a value before the column becomes NOT NULL + UNIQUE.
                migrationBuilder.Sql(
                    $"UPDATE \"{table}\" SET \"PublicId\" = replace(gen_random_uuid()::text, '-', '') WHERE \"PublicId\" IS NULL;");

                migrationBuilder.AlterColumn<string>(
                    name: "PublicId",
                    table: table,
                    type: "character varying(32)",
                    maxLength: 32,
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "character varying(32)",
                    oldMaxLength: 32,
                    oldNullable: true);

                migrationBuilder.CreateIndex(
                    name: $"IX_{table}_PublicId",
                    table: table,
                    column: "PublicId",
                    unique: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransferValueHistories_PublicId",
                table: "TransferValueHistories");

            migrationBuilder.DropIndex(
                name: "IX_SubPositions_PublicId",
                table: "SubPositions");

            migrationBuilder.DropIndex(
                name: "IX_Stadiums_PublicId",
                table: "Stadiums");

            migrationBuilder.DropIndex(
                name: "IX_Selections_PublicId",
                table: "Selections");

            migrationBuilder.DropIndex(
                name: "IX_SelectionManagerHistories_PublicId",
                table: "SelectionManagerHistories");

            migrationBuilder.DropIndex(
                name: "IX_Seasons_PublicId",
                table: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_PublicId",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_Positions_PublicId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_PlayerSelectionHistories_PublicId",
                table: "PlayerSelectionHistories");

            migrationBuilder.DropIndex(
                name: "IX_Persons_PublicId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_MatchStatuses_PublicId",
                table: "MatchStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Matches_PublicId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_MandatDates_PublicId",
                table: "MandatDates");

            migrationBuilder.DropIndex(
                name: "IX_GeoDatas_PublicId",
                table: "GeoDatas");

            migrationBuilder.DropIndex(
                name: "IX_Formations_PublicId",
                table: "Formations");

            migrationBuilder.DropIndex(
                name: "IX_FormationMarks_PublicId",
                table: "FormationMarks");

            migrationBuilder.DropIndex(
                name: "IX_Countries_PublicId",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Competitions_PublicId",
                table: "Competitions");

            migrationBuilder.DropIndex(
                name: "IX_Cities_PublicId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "TransferValueHistories");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "SubPositions");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Stadiums");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Selections");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "SelectionManagerHistories");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "PlayerSelectionHistories");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "MatchStatuses");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "MandatDates");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "GeoDatas");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Formations");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "FormationMarks");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Competitions");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Cities");
        }
    }
}
