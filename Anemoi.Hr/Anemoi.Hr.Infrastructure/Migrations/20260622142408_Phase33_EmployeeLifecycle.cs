using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase33_EmployeeLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeHistories",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOrganizationHistories",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    GradeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ChangeReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOrganizationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOrganizationHistories_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeOrganizationHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeOrganizationHistories_Employees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EmployeeOrganizationHistories_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSeparations",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeparationTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeparationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastWorkingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSeparations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSeparations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTransfers",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetDepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetPositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewComment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTransfers_Departments_SourceDepartmentId",
                        column: x => x.SourceDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeTransfers_Departments_TargetDepartmentId",
                        column: x => x.TargetDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeTransfers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeTransfers_Positions_SourcePositionId",
                        column: x => x.SourcePositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeTransfers_Positions_TargetPositionId",
                        column: x => x.TargetPositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProbationRecords",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReviewerEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProbationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProbationRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProbationRecords_Employees_ReviewerEmployeeId",
                        column: x => x.ReviewerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHistories_CorrelationId",
                schema: "Hr",
                table: "EmployeeHistories",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHistories_EmployeeId_OccurredAt",
                schema: "Hr",
                table: "EmployeeHistories",
                columns: new[] { "EmployeeId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHistories_EntityType",
                schema: "Hr",
                table: "EmployeeHistories",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOrganizationHistories_DepartmentId",
                schema: "Hr",
                table: "EmployeeOrganizationHistories",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOrganizationHistories_EmployeeId_EffectiveDate",
                schema: "Hr",
                table: "EmployeeOrganizationHistories",
                columns: new[] { "EmployeeId", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOrganizationHistories_EndDate",
                schema: "Hr",
                table: "EmployeeOrganizationHistories",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOrganizationHistories_ManagerEmployeeId",
                schema: "Hr",
                table: "EmployeeOrganizationHistories",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOrganizationHistories_PositionId",
                schema: "Hr",
                table: "EmployeeOrganizationHistories",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSeparations_EmployeeId_StatusCode",
                schema: "Hr",
                table: "EmployeeSeparations",
                columns: new[] { "EmployeeId", "StatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSeparations_LastWorkingDate",
                schema: "Hr",
                table: "EmployeeSeparations",
                column: "LastWorkingDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSeparations_SeparationDate",
                schema: "Hr",
                table: "EmployeeSeparations",
                column: "SeparationDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTransfers_EffectiveDate",
                schema: "Hr",
                table: "EmployeeTransfers",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTransfers_EmployeeId_StatusCode",
                schema: "Hr",
                table: "EmployeeTransfers",
                columns: new[] { "EmployeeId", "StatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTransfers_SourceDepartmentId",
                schema: "Hr",
                table: "EmployeeTransfers",
                column: "SourceDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTransfers_SourcePositionId",
                schema: "Hr",
                table: "EmployeeTransfers",
                column: "SourcePositionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTransfers_TargetDepartmentId",
                schema: "Hr",
                table: "EmployeeTransfers",
                column: "TargetDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTransfers_TargetPositionId",
                schema: "Hr",
                table: "EmployeeTransfers",
                column: "TargetPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProbationRecords_EmployeeId_StartDate",
                schema: "Hr",
                table: "ProbationRecords",
                columns: new[] { "EmployeeId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProbationRecords_ReviewerEmployeeId",
                schema: "Hr",
                table: "ProbationRecords",
                column: "ReviewerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProbationRecords_StatusCode",
                schema: "Hr",
                table: "ProbationRecords",
                column: "StatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeHistories",
                schema: "Hr");

            migrationBuilder.DropTable(
                name: "EmployeeOrganizationHistories",
                schema: "Hr");

            migrationBuilder.DropTable(
                name: "EmployeeSeparations",
                schema: "Hr");

            migrationBuilder.DropTable(
                name: "EmployeeTransfers",
                schema: "Hr");

            migrationBuilder.DropTable(
                name: "ProbationRecords",
                schema: "Hr");
        }
    }
}
