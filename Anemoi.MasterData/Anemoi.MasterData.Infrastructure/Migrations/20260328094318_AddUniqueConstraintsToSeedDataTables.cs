using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.MasterData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsToSeedDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SeedTemplates_Name",
                table: "SeedTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeedServers_Name",
                table: "SeedServers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeedFunctions_Name",
                table: "SeedFunctions",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SeedTemplates_Name",
                table: "SeedTemplates");

            migrationBuilder.DropIndex(
                name: "IX_SeedServers_Name",
                table: "SeedServers");

            migrationBuilder.DropIndex(
                name: "IX_SeedFunctions_Name",
                table: "SeedFunctions");
        }
    }
}
