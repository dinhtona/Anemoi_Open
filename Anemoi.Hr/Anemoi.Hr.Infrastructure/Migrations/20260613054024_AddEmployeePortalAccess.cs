using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    public partial class AddEmployeePortalAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeePortalAccesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAccessAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePortalAccesses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePortalAccesses_EmployeeId",
                table: "EmployeePortalAccesses",
                column: "EmployeeId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeePortalAccesses");
        }
    }
}
