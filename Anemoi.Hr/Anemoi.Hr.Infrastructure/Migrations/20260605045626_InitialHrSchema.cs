using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialHrSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeavePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LeaveTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MonthlyAccrualDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    AnnualMaxDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    AllowCarryForward = table.Column<bool>(type: "boolean", nullable: false),
                    MaxCarryForwardDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeavePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DepartmentTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ParentDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PositionTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WorkEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PersonalEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    JoinDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EmploymentStatusCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmploymentTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PrimaryDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    DirectManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_PrimaryDepartmentId",
                        column: x => x.PrimaryDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_DirectManagerEmployeeId",
                        column: x => x.DirectManagerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Positions_PrimaryPositionId",
                        column: x => x.PrimaryPositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDepartmentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDepartmentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDepartmentHistories_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeDepartmentHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeGradeHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    GradeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeGradeHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeGradeHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeManagerHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeManagerHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeManagerHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeManagerHistories_Employees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePositionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePositionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePositionHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePositionHistories_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveAccrualRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeavePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    YearMonth = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    AccruedDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveAccrualRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveAccrualRuns_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveAccrualRuns_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeavePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    OpeningDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    AccruedDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    UsedDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    PendingDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    AdjustedDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    RemainingDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeavePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApproverEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestedDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    StatusCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_ApproverEmployeeId",
                        column: x => x.ApproverEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeavePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveBalanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Days = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    BalanceAfterDays = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveTransactions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveTransactions_LeaveBalances_LeaveBalanceId",
                        column: x => x.LeaveBalanceId,
                        principalTable: "LeaveBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveTransactions_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveTransactions_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerEmployeeId",
                table: "Departments",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentHistories_DepartmentId",
                table: "EmployeeDepartmentHistories",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeDepartmentHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeGradeHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeGradeHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeManagerHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeManagerHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeManagerHistories_ManagerEmployeeId",
                table: "EmployeeManagerHistories",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePositionHistories_EmployeeId_EffectiveFrom",
                table: "EmployeePositionHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePositionHistories_PositionId",
                table: "EmployeePositionHistories",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DirectManagerEmployeeId",
                table: "Employees",
                column: "DirectManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PrimaryDepartmentId",
                table: "Employees",
                column: "PrimaryDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PrimaryPositionId",
                table: "Employees",
                column: "PrimaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_WorkEmail",
                table: "Employees",
                column: "WorkEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAccrualRuns_EmployeeId_LeavePolicyId_YearMonth",
                table: "LeaveAccrualRuns",
                columns: new[] { "EmployeeId", "LeavePolicyId", "YearMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAccrualRuns_LeavePolicyId",
                table: "LeaveAccrualRuns",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_LeavePolicyId_Year",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "LeavePolicyId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_LeavePolicyId",
                table: "LeaveBalances",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_Code",
                table: "LeavePolicies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApproverEmployeeId",
                table: "LeaveRequests",
                column: "ApproverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId_StatusCode",
                table: "LeaveRequests",
                columns: new[] { "EmployeeId", "StatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeavePolicyId",
                table: "LeaveRequests",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_StartDate_EndDate",
                table: "LeaveRequests",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTransactions_EmployeeId_CreatedAt",
                table: "LeaveTransactions",
                columns: new[] { "EmployeeId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTransactions_LeaveBalanceId",
                table: "LeaveTransactions",
                column: "LeaveBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTransactions_LeavePolicyId",
                table: "LeaveTransactions",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTransactions_LeaveRequestId",
                table: "LeaveTransactions",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Code",
                table: "Positions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_DepartmentId",
                table: "Positions",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerEmployeeId",
                table: "Departments",
                column: "ManagerEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerEmployeeId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "EmployeeDepartmentHistories");

            migrationBuilder.DropTable(
                name: "EmployeeGradeHistories");

            migrationBuilder.DropTable(
                name: "EmployeeManagerHistories");

            migrationBuilder.DropTable(
                name: "EmployeePositionHistories");

            migrationBuilder.DropTable(
                name: "LeaveAccrualRuns");

            migrationBuilder.DropTable(
                name: "LeaveTransactions");

            migrationBuilder.DropTable(
                name: "LeaveBalances");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "LeavePolicies");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
