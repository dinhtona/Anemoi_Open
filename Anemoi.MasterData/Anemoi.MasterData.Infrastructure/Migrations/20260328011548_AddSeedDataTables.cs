using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.MasterData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeedServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ConnectionString = table.Column<string>(type: "text", nullable: false),
                    Environment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeedTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ConfigJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeedFunctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeedServerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SeedTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TablesJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedFunctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeedFunctions_SeedServers_SeedServerId",
                        column: x => x.SeedServerId,
                        principalTable: "SeedServers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeedFunctions_SeedTemplates_SeedTemplateId",
                        column: x => x.SeedTemplateId,
                        principalTable: "SeedTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeedFunctions_SeedServerId",
                table: "SeedFunctions",
                column: "SeedServerId");

            migrationBuilder.CreateIndex(
                name: "IX_SeedFunctions_SeedTemplateId",
                table: "SeedFunctions",
                column: "SeedTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeedFunctions");

            migrationBuilder.DropTable(
                name: "SeedServers");

            migrationBuilder.DropTable(
                name: "SeedTemplates");
        }
    }
}
