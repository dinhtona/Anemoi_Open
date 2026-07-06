using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hr_tax_calculation_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TaxType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TaxRuleSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxRuleSetVersion = table.Column<int>(type: "integer", nullable: false),
                    GrossIncomeSnapshot = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxableIncomeSnapshot = table.Column<decimal>(type: "numeric", nullable: false),
                    DeductionSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    BracketSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    CalculationResultJson = table.Column<string>(type: "text", nullable: true),
                    TotalTaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CalculationPeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    CalculationPeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalculatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_tax_calculation_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hr_tax_rule_sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TaxType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_tax_rule_sets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hr_tax_brackets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxRuleSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ToAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    QuickDeductionAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_tax_brackets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_tax_brackets_hr_tax_rule_sets_TaxRuleSetId",
                        column: x => x.TaxRuleSetId,
                        principalTable: "hr_tax_rule_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hr_tax_deduction_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxRuleSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_tax_deduction_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_tax_deduction_rules_hr_tax_rule_sets_TaxRuleSetId",
                        column: x => x.TaxRuleSetId,
                        principalTable: "hr_tax_rule_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hr_tax_brackets_TaxRuleSetId_SortOrder",
                table: "hr_tax_brackets",
                columns: new[] { "TaxRuleSetId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_tax_calculation_snapshots_EmployeeId_CalculationPeriodSt~",
                table: "hr_tax_calculation_snapshots",
                columns: new[] { "EmployeeId", "CalculationPeriodStart", "CalculationPeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_tax_calculation_snapshots_SourceModule_SourceReferenceId",
                table: "hr_tax_calculation_snapshots",
                columns: new[] { "SourceModule", "SourceReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_tax_deduction_rules_TaxRuleSetId_DeductionType",
                table: "hr_tax_deduction_rules",
                columns: new[] { "TaxRuleSetId", "DeductionType" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_tax_rule_sets_CountryCode_TaxType_EffectiveFrom_Effectiv~",
                table: "hr_tax_rule_sets",
                columns: new[] { "CountryCode", "TaxType", "EffectiveFrom", "EffectiveTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_tax_brackets");

            migrationBuilder.DropTable(
                name: "hr_tax_calculation_snapshots");

            migrationBuilder.DropTable(
                name: "hr_tax_deduction_rules");

            migrationBuilder.DropTable(
                name: "hr_tax_rule_sets");
        }
    }
}
