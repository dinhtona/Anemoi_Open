using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayslipMVP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payslips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BaseSalarySnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DailyRateSnapshot = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PaidWorkingDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    PaidLeaveDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    UnpaidLeaveDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    BasePayAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllowanceTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeductionTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossPay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetPay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payslips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payslips_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payslips_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_EmployeeId",
                table: "Payslips",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_PayrollRunId_EmployeeId",
                table: "Payslips",
                columns: new[] { "PayrollRunId", "EmployeeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payslips");
        }
    }
}
