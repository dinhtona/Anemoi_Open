using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentTransferStabilization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeDepartmentHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeDepartmentHistories");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Employees",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "EmployeeDepartmentHistories",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeDepartmentHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeDepartmentHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeDepartmentHistories");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "EmployeeDepartmentHistories");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeDepartmentHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" });
        }
    }
}
