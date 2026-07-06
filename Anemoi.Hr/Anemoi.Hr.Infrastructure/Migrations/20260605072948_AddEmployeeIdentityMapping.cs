using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeIdentityMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdentityUserId",
                table: "Employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeIdentityLinkLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IdentityUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    MatchStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeIdentityLinkLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeIdentityLinkLogs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IdentityUserId",
                table: "Employees",
                column: "IdentityUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeIdentityLinkLogs_CreatedAt",
                table: "EmployeeIdentityLinkLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeIdentityLinkLogs_EmployeeId",
                table: "EmployeeIdentityLinkLogs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeIdentityLinkLogs_IdentityUserId",
                table: "EmployeeIdentityLinkLogs",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeIdentityLinkLogs_MatchStatus",
                table: "EmployeeIdentityLinkLogs",
                column: "MatchStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeIdentityLinkLogs");

            migrationBuilder.DropIndex(
                name: "IX_Employees_IdentityUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Employees");
        }
    }
}
