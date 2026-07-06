using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hr_insurance_calculation_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    InsuranceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    RuleSetVersion = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CalculationPeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    CalculationPeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    InsurableSalarySnapshot = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalEmployeeContribution = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalEmployerContribution = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalContribution = table.Column<decimal>(type: "numeric", nullable: false),
                    RuleSetSnapshotJson = table.Column<string>(type: "text", nullable: false),
                    CalculationResultJson = table.Column<string>(type: "text", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalculatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceReferenceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_insurance_calculation_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hr_insurance_rule_sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    InsuranceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
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
                    table.PrimaryKey("PK_hr_insurance_rule_sets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hr_insurance_calculation_snapshot_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContributionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContributionBase = table.Column<decimal>(type: "numeric", nullable: false),
                    EmployeeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    EmployerRate = table.Column<decimal>(type: "numeric", nullable: false),
                    EmployeeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    EmployerAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_insurance_calculation_snapshot_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_insurance_calculation_snapshot_items_hr_insurance_calcul~",
                        column: x => x.SnapshotId,
                        principalTable: "hr_insurance_calculation_snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hr_insurance_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PerformedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_insurance_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_insurance_audit_logs_hr_insurance_rule_sets_RuleSetId",
                        column: x => x.RuleSetId,
                        principalTable: "hr_insurance_rule_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "hr_insurance_contribution_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContributionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmployeeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    EmployerRate = table.Column<decimal>(type: "numeric", nullable: false),
                    CeilingAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    MinimumAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalaryBasis = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_insurance_contribution_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_insurance_contribution_rules_hr_insurance_rule_sets_Rule~",
                        column: x => x.RuleSetId,
                        principalTable: "hr_insurance_rule_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hr_insurance_audit_logs_RuleSetId",
                table: "hr_insurance_audit_logs",
                column: "RuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_hr_insurance_calculation_snapshot_items_SnapshotId_SortOrder",
                table: "hr_insurance_calculation_snapshot_items",
                columns: new[] { "SnapshotId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_insurance_calculation_snapshots_EmployeeId_CalculationPe~",
                table: "hr_insurance_calculation_snapshots",
                columns: new[] { "EmployeeId", "CalculationPeriodStart", "CalculationPeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_insurance_calculation_snapshots_SourceModule_SourceRefer~",
                table: "hr_insurance_calculation_snapshots",
                columns: new[] { "SourceModule", "SourceReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_insurance_contribution_rules_RuleSetId_ContributionType",
                table: "hr_insurance_contribution_rules",
                columns: new[] { "RuleSetId", "ContributionType" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_insurance_contribution_rules_RuleSetId_SortOrder",
                table: "hr_insurance_contribution_rules",
                columns: new[] { "RuleSetId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_insurance_rule_sets_CountryCode_InsuranceType_EffectiveF~",
                table: "hr_insurance_rule_sets",
                columns: new[] { "CountryCode", "InsuranceType", "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_hr_insurance_rule_sets_CountryCode_InsuranceType_Status",
                table: "hr_insurance_rule_sets",
                columns: new[] { "CountryCode", "InsuranceType", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_insurance_audit_logs");

            migrationBuilder.DropTable(
                name: "hr_insurance_calculation_snapshot_items");

            migrationBuilder.DropTable(
                name: "hr_insurance_contribution_rules");

            migrationBuilder.DropTable(
                name: "hr_insurance_calculation_snapshots");

            migrationBuilder.DropTable(
                name: "hr_insurance_rule_sets");
        }
    }
}
