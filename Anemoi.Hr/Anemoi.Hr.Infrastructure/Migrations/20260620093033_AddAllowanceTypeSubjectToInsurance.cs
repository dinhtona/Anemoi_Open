using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowanceTypeSubjectToInsurance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsTaxable",
                table: "AllowanceTypes",
                newName: "Taxable");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "WorkflowRoleAssignments",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Positions",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Departments",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "SubjectToInsurance",
                table: "AllowanceTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AnnualEntitlement = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    CarryForwardAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    MaxCarryForwardDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OvertimeRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WeekdayMultiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    WeekendMultiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    HolidayMultiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterDataLeavePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicableGradeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AnnualEntitlement = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterDataLeavePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterDataLeavePolicies_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Code",
                table: "LeaveTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasterDataLeavePolicies_Code",
                table: "MasterDataLeavePolicies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasterDataLeavePolicies_LeaveTypeId_ApplicableGradeCode",
                table: "MasterDataLeavePolicies",
                columns: new[] { "LeaveTypeId", "ApplicableGradeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRules_Code",
                table: "OvertimeRules",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowRoleAssignments_Employees_EmployeeId",
                table: "WorkflowRoleAssignments",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowRoleAssignments_Employees_EmployeeId",
                table: "WorkflowRoleAssignments");

            migrationBuilder.DropTable(
                name: "MasterDataLeavePolicies");

            migrationBuilder.DropTable(
                name: "OvertimeRules");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "WorkflowRoleAssignments");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "SubjectToInsurance",
                table: "AllowanceTypes");

            migrationBuilder.RenameColumn(
                name: "Taxable",
                table: "AllowanceTypes",
                newName: "IsTaxable");
        }
    }
}
