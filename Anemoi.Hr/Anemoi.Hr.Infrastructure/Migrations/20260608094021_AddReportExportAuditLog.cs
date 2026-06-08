using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportExportAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportExportAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReportType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExportedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TotalRecords = table.Column<int>(type: "integer", nullable: false),
                    FiltersJson = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportExportAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExportAuditLogs_ExportedAt",
                table: "ReportExportAuditLogs",
                column: "ExportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExportAuditLogs_ModuleCode_ReportType",
                table: "ReportExportAuditLogs",
                columns: new[] { "ModuleCode", "ReportType" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExportAuditLogs_ExportedBy_ExportedAt",
                table: "ReportExportAuditLogs",
                columns: new[] { "ExportedBy", "ExportedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportExportAuditLogs");
        }
    }
}
