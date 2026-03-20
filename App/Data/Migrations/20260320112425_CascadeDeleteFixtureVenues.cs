using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sport.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteFixtureVenues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vf_fixture",
                table: "venue_fixtures");

            migrationBuilder.AddForeignKey(
                name: "fk_vf_fixture",
                table: "venue_fixtures",
                column: "fixture_id",
                principalTable: "fixtures",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vf_fixture",
                table: "venue_fixtures");

            migrationBuilder.AddForeignKey(
                name: "fk_vf_fixture",
                table: "venue_fixtures",
                column: "fixture_id",
                principalTable: "fixtures",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
