using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RelatedHolidayId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarExceptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyHolidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HolidayDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRecurringAnnual = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyHolidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicHolidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HolidayDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsRecurringAnnual = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicHolidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkingCalendarRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WorkMonday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkTuesday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkWednesday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkThursday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkFriday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkSaturday = table.Column<bool>(type: "boolean", nullable: false),
                    WorkSunday = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingCalendarRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_ExceptionDate",
                table: "CalendarExceptions",
                column: "ExceptionDate");

            migrationBuilder.CreateIndex(
                name: "IX_PublicHolidays_HolidayDate_CountryCode",
                table: "PublicHolidays",
                columns: new[] { "HolidayDate", "CountryCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkingCalendarRules_EffectiveFrom_EffectiveTo_IsActive",
                table: "WorkingCalendarRules",
                columns: new[] { "EffectiveFrom", "EffectiveTo", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarExceptions");

            migrationBuilder.DropTable(
                name: "CompanyHolidays");

            migrationBuilder.DropTable(
                name: "PublicHolidays");

            migrationBuilder.DropTable(
                name: "WorkingCalendarRules");
        }
    }
}
