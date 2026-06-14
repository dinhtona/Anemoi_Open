using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentSnapshotsToPayrollRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_AttendancePeriodId",
                table: "PayrollPeriods",
                column: "AttendancePeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollPeriods_AttendancePeriods_AttendancePeriodId",
                table: "PayrollPeriods",
                column: "AttendancePeriodId",
                principalTable: "AttendancePeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollPeriods_AttendancePeriods_AttendancePeriodId",
                table: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_AttendancePeriodId",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "DepartmentIdSnapshot",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "DepartmentNameSnapshot",
                table: "PayrollRuns");
        }
    }
}
