using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_CreatedTime",
                table: "NotificationHistories",
                column: "CreatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_UserId",
                table: "NotificationHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSubscriptions_UserId_Category",
                table: "NotificationSubscriptions",
                columns: new[] { "UserId", "Category" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationHistories");

            migrationBuilder.DropTable(
                name: "NotificationSubscriptions");
        }
    }
}
