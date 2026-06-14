using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentSnapshotsToPayrollItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentIdSnapshot",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "DepartmentNameSnapshot",
                table: "PayrollRuns");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentIdSnapshot",
                table: "PayrollItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentNameSnapshot",
                table: "PayrollItems",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentIdSnapshot",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "DepartmentNameSnapshot",
                table: "PayrollItems");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentIdSnapshot",
                table: "PayrollRuns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentNameSnapshot",
                table: "PayrollRuns",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
