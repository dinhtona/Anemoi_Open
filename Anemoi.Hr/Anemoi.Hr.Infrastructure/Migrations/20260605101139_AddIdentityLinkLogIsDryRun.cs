using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityLinkLogIsDryRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDryRun",
                table: "EmployeeIdentityLinkLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeIdentityLinkLogs_IsDryRun",
                table: "EmployeeIdentityLinkLogs",
                column: "IsDryRun");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeIdentityLinkLogs_IsDryRun",
                table: "EmployeeIdentityLinkLogs");

            migrationBuilder.DropColumn(
                name: "IsDryRun",
                table: "EmployeeIdentityLinkLogs");
        }
    }
}
