using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.MasterData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedHistoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeedHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeedFunctionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeedHistories_SeedFunctions_SeedFunctionId",
                        column: x => x.SeedFunctionId,
                        principalTable: "SeedFunctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeedHistories_SeedFunctionId",
                table: "SeedHistories",
                column: "SeedFunctionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeedHistories");
        }
    }
}
