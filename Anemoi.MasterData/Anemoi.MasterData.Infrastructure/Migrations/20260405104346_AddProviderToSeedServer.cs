using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.MasterData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderToSeedServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "SeedServers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "SeedServers");
        }
    }
}
