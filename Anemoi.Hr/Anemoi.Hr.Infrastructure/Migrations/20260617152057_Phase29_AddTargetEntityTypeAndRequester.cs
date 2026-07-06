using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase29_AddTargetEntityTypeAndRequester : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecruitmentOpeningId",
                table: "JobPostings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecruitmentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedHeadcount = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PriorityCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequests_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequests_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkflowTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequesterEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequesterUserId = table.Column<Guid>(type: "uuid", maxLength: 128, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecruitmentOpenings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruitmentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PlannedHeadcount = table.Column<int>(type: "integer", nullable: false),
                    FilledHeadcount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitmentOpenings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruitmentOpenings_RecruitmentRequests_RecruitmentRequestId",
                        column: x => x.RecruitmentRequestId,
                        principalTable: "RecruitmentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecruitmentRequestHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruitmentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OldStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitmentRequestHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequestHistories_RecruitmentRequests_Recruitment~",
                        column: x => x.RecruitmentRequestId,
                        principalTable: "RecruitmentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowDefinitionSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ApproverType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApproverValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitionSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowDefinitionSteps_WorkflowDefinitions_WorkflowDefinit~",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ApproverTypeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApproverValueSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApproverUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceSteps_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_RecruitmentOpeningId",
                table: "JobPostings",
                column: "RecruitmentOpeningId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentOpenings_RecruitmentRequestId",
                table: "RecruitmentOpenings",
                column: "RecruitmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestHistories_PerformedAt",
                table: "RecruitmentRequestHistories",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequestHistories_RecruitmentRequestId",
                table: "RecruitmentRequestHistories",
                column: "RecruitmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequests_DepartmentId",
                table: "RecruitmentRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequests_PositionId",
                table: "RecruitmentRequests",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequests_RequestedAt",
                table: "RecruitmentRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequests_RequestNumber",
                table: "RecruitmentRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequests_Status",
                table: "RecruitmentRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_Code",
                table: "WorkflowDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_IsActive",
                table: "WorkflowDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_TargetEntityType",
                table: "WorkflowDefinitions",
                column: "TargetEntityType",
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_WorkflowTypeCode",
                table: "WorkflowDefinitions",
                column: "WorkflowTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitionSteps_WorkflowDefinitionId_Sequence",
                table: "WorkflowDefinitionSteps",
                columns: new[] { "WorkflowDefinitionId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_PerformedAt",
                table: "WorkflowHistories",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_WorkflowInstanceId",
                table: "WorkflowHistories",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_EntityType_EntityId",
                table: "WorkflowInstances",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_StartedBy",
                table: "WorkflowInstances",
                column: "StartedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_Status",
                table: "WorkflowInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_WorkflowDefinitionId",
                table: "WorkflowInstances",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceSteps_Status",
                table: "WorkflowInstanceSteps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceSteps_WorkflowInstanceId",
                table: "WorkflowInstanceSteps",
                column: "WorkflowInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostings_RecruitmentOpenings_RecruitmentOpeningId",
                table: "JobPostings",
                column: "RecruitmentOpeningId",
                principalTable: "RecruitmentOpenings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPostings_RecruitmentOpenings_RecruitmentOpeningId",
                table: "JobPostings");

            migrationBuilder.DropTable(
                name: "RecruitmentOpenings");

            migrationBuilder.DropTable(
                name: "RecruitmentRequestHistories");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitionSteps");

            migrationBuilder.DropTable(
                name: "WorkflowHistories");

            migrationBuilder.DropTable(
                name: "WorkflowInstanceSteps");

            migrationBuilder.DropTable(
                name: "RecruitmentRequests");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_JobPostings_RecruitmentOpeningId",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "RecruitmentOpeningId",
                table: "JobPostings");
        }
    }
}
