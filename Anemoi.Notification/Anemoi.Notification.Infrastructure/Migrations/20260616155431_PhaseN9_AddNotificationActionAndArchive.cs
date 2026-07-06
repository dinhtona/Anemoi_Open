using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PhaseN9_AddNotificationActionAndArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AggregateId",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AggregateType",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "NotificationHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedBy",
                table: "NotificationHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "NotificationHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowState",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowType",
                table: "NotificationHistories",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationActionAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExecutedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Result = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClientIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationActionAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequiresConfirmation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationActions_NotificationHistories_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "NotificationHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_UserId_IsArchived",
                table: "NotificationHistories",
                columns: new[] { "UserId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationActionAudits_ExecutedAt",
                table: "NotificationActionAudits",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationActionAudits_ExecutedBy",
                table: "NotificationActionAudits",
                column: "ExecutedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationActionAudits_NotificationId",
                table: "NotificationActionAudits",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationActions_NotificationId",
                table: "NotificationActions",
                column: "NotificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationActionAudits");

            migrationBuilder.DropTable(
                name: "NotificationActions");

            migrationBuilder.DropIndex(
                name: "IX_NotificationHistories_UserId_IsArchived",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "AggregateId",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "AggregateType",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "ArchivedBy",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "WorkflowState",
                table: "NotificationHistories");

            migrationBuilder.DropColumn(
                name: "WorkflowType",
                table: "NotificationHistories");
        }
    }
}
