using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PhaseN7_AddNotificationPreferenceAndHide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HiddenAt",
                table: "NotificationHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "NotificationHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EnableInApp = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EnableEmail = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_UserId_CreatedTime",
                table: "NotificationHistories",
                columns: new[] { "UserId", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_UserId_IsHidden",
                table: "NotificationHistories",
                columns: new[] { "UserId", "IsHidden" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropIndex(
                name: "IX_NotificationHistories_UserId_CreatedTime",
                table: "NotificationHistories");

            migrationBuilder.DropIndex(
                name: "IX_NotificationHistories_UserId_IsHidden",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "HiddenAt",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "NotificationHistories");
        }
    }
}
