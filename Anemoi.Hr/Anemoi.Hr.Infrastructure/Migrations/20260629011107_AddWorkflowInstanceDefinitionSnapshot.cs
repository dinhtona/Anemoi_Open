using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowInstanceDefinitionSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkflowDefinitionName",
                table: "WorkflowInstances",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowDefinitionVersion",
                table: "WorkflowInstances",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionName",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionVersion",
                table: "WorkflowInstances");
        }
    }
}
