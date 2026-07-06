using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    public partial class StandardizeTableNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OvertimeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OvertimeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_CreatedAt",
                table: "OvertimeRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_EmployeeId_OvertimeDate",
                table: "OvertimeRequests",
                columns: new[] { "EmployeeId", "OvertimeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_OvertimeDate",
                table: "OvertimeRequests",
                column: "OvertimeDate");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_Status",
                table: "OvertimeRequests",
                column: "Status");

            migrationBuilder.RenameTable(
                name: "hr_tax_rule_sets",
                newName: "TaxRuleSets");

            migrationBuilder.RenameTable(
                name: "hr_tax_brackets",
                newName: "TaxBrackets");

            migrationBuilder.RenameTable(
                name: "hr_tax_deduction_rules",
                newName: "TaxDeductionRules");

            migrationBuilder.RenameTable(
                name: "hr_tax_calculation_snapshots",
                newName: "TaxCalculationSnapshots");

            migrationBuilder.RenameTable(
                name: "hr_insurance_rule_sets",
                newName: "InsuranceRuleSets");

            migrationBuilder.RenameTable(
                name: "hr_insurance_contribution_rules",
                newName: "InsuranceContributionRules");

            migrationBuilder.RenameTable(
                name: "hr_insurance_calculation_snapshots",
                newName: "InsuranceCalculationSnapshots");

            migrationBuilder.RenameTable(
                name: "hr_insurance_calculation_snapshot_items",
                newName: "InsuranceCalculationSnapshotItems");

            migrationBuilder.RenameTable(
                name: "hr_insurance_audit_logs",
                newName: "InsuranceAuditLogs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "InsuranceAuditLogs",
                newName: "hr_insurance_audit_logs");

            migrationBuilder.RenameTable(
                name: "InsuranceCalculationSnapshotItems",
                newName: "hr_insurance_calculation_snapshot_items");

            migrationBuilder.RenameTable(
                name: "InsuranceCalculationSnapshots",
                newName: "hr_insurance_calculation_snapshots");

            migrationBuilder.RenameTable(
                name: "InsuranceContributionRules",
                newName: "hr_insurance_contribution_rules");

            migrationBuilder.RenameTable(
                name: "InsuranceRuleSets",
                newName: "hr_insurance_rule_sets");

            migrationBuilder.RenameTable(
                name: "TaxCalculationSnapshots",
                newName: "hr_tax_calculation_snapshots");

            migrationBuilder.RenameTable(
                name: "TaxDeductionRules",
                newName: "hr_tax_deduction_rules");

            migrationBuilder.RenameTable(
                name: "TaxBrackets",
                newName: "hr_tax_brackets");

            migrationBuilder.RenameTable(
                name: "TaxRuleSets",
                newName: "hr_tax_rule_sets");

            migrationBuilder.DropTable(
                name: "OvertimeRequests");
        }
    }
}
