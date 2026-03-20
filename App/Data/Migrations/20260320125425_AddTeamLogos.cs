using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sport.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamLogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "away_team_logo",
                table: "fixtures",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "home_team_logo",
                table: "fixtures",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "away_team_logo",
                table: "fixtures");

            migrationBuilder.DropColumn(
                name: "home_team_logo",
                table: "fixtures");
        }
    }
}
