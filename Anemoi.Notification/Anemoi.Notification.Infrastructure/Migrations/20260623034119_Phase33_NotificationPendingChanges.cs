using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase33_NotificationPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_CorrelationId",
                table: "NotificationHistories",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationHistories_CorrelationId",
                table: "NotificationHistories");
        }
    }
}
