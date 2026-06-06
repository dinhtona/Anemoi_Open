using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceToPayrollIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AttendancePeriodId",
                table: "PayrollPeriods",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AttendanceSummaryId",
                table: "PayrollItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePayAmount",
                table: "PayrollItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalarySnapshot",
                table: "PayrollItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyRateSnapshot",
                table: "PayrollItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidLeaveDays",
                table: "PayrollItems",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidWorkingDays",
                table: "PayrollItems",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnpaidLeaveDays",
                table: "PayrollItems",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AttendanceSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendancePeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkedDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    WorkedHours = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    LeaveDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    AbsentDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    HolidayDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    PaidWorkingDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    PaidLeaveDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    UnpaidLeaveDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceSummaries_AttendancePeriods_AttendancePeriodId",
                        column: x => x.AttendancePeriodId,
                        principalTable: "AttendancePeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceSummaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSummaries_AttendancePeriodId_EmployeeId",
                table: "AttendanceSummaries",
                columns: new[] { "AttendancePeriodId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSummaries_EmployeeId",
                table: "AttendanceSummaries",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceSummaries");

            migrationBuilder.DropColumn(
                name: "AttendancePeriodId",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "AttendanceSummaryId",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "BasePayAmount",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "BaseSalarySnapshot",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "DailyRateSnapshot",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "PaidLeaveDays",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "PaidWorkingDays",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "UnpaidLeaveDays",
                table: "PayrollItems");
        }
    }
}
