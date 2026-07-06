using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveRequestDashboardIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_StatusCode_StartDate_EndDate",
                table: "LeaveRequests",
                columns: new[] { "StatusCode", "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_StatusCode_StartDate_EndDate",
                table: "LeaveRequests");
        }
    }
}
