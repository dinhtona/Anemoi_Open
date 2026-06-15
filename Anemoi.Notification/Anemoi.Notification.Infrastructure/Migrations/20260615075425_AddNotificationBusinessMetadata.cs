using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationBusinessMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActionUrl",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CausationId",
                table: "NotificationHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentLocalizationArgs",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentLocalizationKey",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "NotificationHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeduplicationKey",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleLocalizationArgs",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleLocalizationKey",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_UserId_DeduplicationKey",
                table: "NotificationHistories",
                columns: new[] { "UserId", "DeduplicationKey" },
                unique: true,
                filter: "\"DeduplicationKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationHistories_UserId_DeduplicationKey",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "ActionUrl",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "CausationId",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "ContentLocalizationArgs",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "ContentLocalizationKey",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "DeduplicationKey",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "TitleLocalizationArgs",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "TitleLocalizationKey",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "NotificationHistories");
        }
    }
}
