using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sport.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vf_fixture",
                table: "venue_fixtures");

            migrationBuilder.DropForeignKey(
                name: "fk_vf_venue",
                table: "venue_fixtures");

            migrationBuilder.CreateIndex(
                name: "idx_venues_name",
                table: "venues",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_fixtures_deleted_at",
                table: "fixtures",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_fixtures_sport_starts",
                table: "fixtures",
                columns: new[] { "sport_type", "starts_at" });

            migrationBuilder.AddForeignKey(
                name: "fk_vf_fixture",
                table: "venue_fixtures",
                column: "fixture_id",
                principalTable: "fixtures",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_vf_venue",
                table: "venue_fixtures",
                column: "venue_id",
                principalTable: "venues",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vf_fixture",
                table: "venue_fixtures");

            migrationBuilder.DropForeignKey(
                name: "fk_vf_venue",
                table: "venue_fixtures");

            migrationBuilder.DropIndex(
                name: "idx_venues_name",
                table: "venues");

            migrationBuilder.DropIndex(
                name: "idx_fixtures_deleted_at",
                table: "fixtures");

            migrationBuilder.DropIndex(
                name: "idx_fixtures_sport_starts",
                table: "fixtures");

            migrationBuilder.AddForeignKey(
                name: "fk_vf_fixture",
                table: "venue_fixtures",
                column: "fixture_id",
                principalTable: "fixtures",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vf_venue",
                table: "venue_fixtures",
                column: "venue_id",
                principalTable: "venues",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
