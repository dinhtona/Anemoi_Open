using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase25_FinalModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Hr");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "InterviewFeedbacks",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "CandidateApplicationStageHistories",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "OnboardingInstances",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForceCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ForceCompletedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForceCompleteReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingPlanTemplates",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingPlanTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingTasks",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssigneeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssigneeRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssignedUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssignedUserDisplayNameSnapshot = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReassignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReassignedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompletedNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SkippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SkippedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReopenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReopenedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReopenedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InstanceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingTasks_OnboardingInstances_InstanceId",
                        column: x => x.InstanceId,
                        principalSchema: "Hr",
                        principalTable: "OnboardingInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingTaskTemplates",
                schema: "Hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssigneeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssigneeRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OffsetDays = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingTaskTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingTaskTemplates_OnboardingPlanTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "Hr",
                        principalTable: "OnboardingPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingInstances_EmployeeId",
                schema: "Hr",
                table: "OnboardingInstances",
                column: "EmployeeId",
                unique: true,
                filter: "\"Status\" IN ('Draft', 'InProgress')");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingInstances_StartDate",
                schema: "Hr",
                table: "OnboardingInstances",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingInstances_Status",
                schema: "Hr",
                table: "OnboardingInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingPlanTemplates_Name",
                schema: "Hr",
                table: "OnboardingPlanTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingTasks_AssignedUserId_Status",
                schema: "Hr",
                table: "OnboardingTasks",
                columns: new[] { "AssignedUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingTasks_DueDate",
                schema: "Hr",
                table: "OnboardingTasks",
                column: "DueDate",
                filter: "\"Status\" = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingTasks_InstanceId",
                schema: "Hr",
                table: "OnboardingTasks",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingTaskTemplates_TemplateId",
                schema: "Hr",
                table: "OnboardingTaskTemplates",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnboardingTasks",
                schema: "Hr");

            migrationBuilder.DropTable(
                name: "OnboardingTaskTemplates",
                schema: "Hr");

            migrationBuilder.DropTable(
                name: "OnboardingInstances",
                schema: "Hr");

            migrationBuilder.DropTable(
                name: "OnboardingPlanTemplates",
                schema: "Hr");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "CandidateApplicationStageHistories");
        }
    }
}
