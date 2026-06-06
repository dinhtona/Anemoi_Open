using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayrollDomainAndCQRS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePayAmount",
                table: "PayrollRuns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyRate",
                table: "PayrollRuns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "PayrollRuns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidWorkingDays",
                table: "PayrollRuns",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardWorkingDays",
                table: "PayrollRuns",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductionAmount",
                table: "PayrollRuns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnpaidLeaveDays",
                table: "PayrollRuns",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardWorkingDays",
                table: "PayrollPeriods",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ItemTypeCode",
                table: "PayrollItems",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePayAmount",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "DailyRate",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "PaidWorkingDays",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "StandardWorkingDays",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "TotalDeductionAmount",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "UnpaidLeaveDays",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "StandardWorkingDays",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "ItemTypeCode",
                table: "PayrollItems");
        }
    }
}
