using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase33_EmployeeLifecyclePendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceGradeCode",
                schema: "Hr",
                table: "EmployeeTransfers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetGradeCode",
                schema: "Hr",
                table: "EmployeeTransfers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "GradeCode",
                table: "Employees",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceGradeCode",
                schema: "Hr",
                table: "EmployeeTransfers");

            migrationBuilder.DropColumn(
                name: "TargetGradeCode",
                schema: "Hr",
                table: "EmployeeTransfers");

            migrationBuilder.AlterColumn<string>(
                name: "GradeCode",
                table: "Employees",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
