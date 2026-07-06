using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase30_OrganizationHierarchyEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApproverEmployeeId",
                table: "WorkflowInstanceSteps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkflowRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowRoleAssignments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRoleAssignments_EmployeeId",
                table: "WorkflowRoleAssignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRoleAssignments_Role",
                table: "WorkflowRoleAssignments",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowRoleAssignments_Role_EmployeeId",
                table: "WorkflowRoleAssignments",
                columns: new[] { "Role", "EmployeeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowRoleAssignments");

            migrationBuilder.DropColumn(
                name: "ApproverEmployeeId",
                table: "WorkflowInstanceSteps");
        }
    }
}
