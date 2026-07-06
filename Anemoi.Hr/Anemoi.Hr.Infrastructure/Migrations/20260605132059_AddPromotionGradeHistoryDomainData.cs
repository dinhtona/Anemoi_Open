using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionGradeHistoryDomainData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeePositionHistories_EmployeeId_EffectiveFrom",
                table: "EmployeePositionHistories");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeGradeHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeGradeHistories");

            migrationBuilder.AddColumn<string>(
                name: "GradeCode",
                table: "Employees",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EmployeePositionHistories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OldPositionId",
                table: "EmployeePositionHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "EmployeePositionHistories",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EmployeeGradeHistories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldGradeCode",
                table: "EmployeeGradeHistories",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "EmployeeGradeHistories",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.Sql("""
                UPDATE "EmployeePositionHistories" AS current
                SET "OldPositionId" = previous."PositionId"
                FROM (
                    SELECT "Id",
                           LAG("PositionId") OVER (PARTITION BY "EmployeeId" ORDER BY "EffectiveFrom") AS "PositionId"
                    FROM "EmployeePositionHistories"
                ) AS previous
                WHERE current."Id" = previous."Id"
                  AND previous."PositionId" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE "EmployeePositionHistories"
                SET "CreatedBy" = 'system:migration'
                WHERE "CreatedBy" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE "EmployeeGradeHistories" AS current
                SET "OldGradeCode" = previous."GradeCode"
                FROM (
                    SELECT "Id",
                           LAG("GradeCode") OVER (PARTITION BY "EmployeeId" ORDER BY "EffectiveFrom") AS "GradeCode"
                    FROM "EmployeeGradeHistories"
                ) AS previous
                WHERE current."Id" = previous."Id"
                  AND previous."GradeCode" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE "EmployeeGradeHistories"
                SET "CreatedBy" = 'system:migration'
                WHERE "CreatedBy" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE "Employees" AS employee
                SET "GradeCode" = current_grade."GradeCode"
                FROM (
                    SELECT DISTINCT ON ("EmployeeId")
                           "EmployeeId",
                           "GradeCode"
                    FROM "EmployeeGradeHistories"
                    WHERE "EffectiveFrom" <= CURRENT_DATE
                      AND ("EffectiveTo" IS NULL OR "EffectiveTo" >= CURRENT_DATE)
                    ORDER BY "EmployeeId", "EffectiveFrom" DESC
                ) AS current_grade
                WHERE employee."Id" = current_grade."EmployeeId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePositionHistories_EmployeeId_EffectiveFrom",
                table: "EmployeePositionHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePositionHistories_OldPositionId",
                table: "EmployeePositionHistories",
                column: "OldPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeGradeHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeGradeHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePositionHistories_Positions_OldPositionId",
                table: "EmployeePositionHistories",
                column: "OldPositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePositionHistories_Positions_OldPositionId",
                table: "EmployeePositionHistories");

            migrationBuilder.DropIndex(
                name: "IX_EmployeePositionHistories_EmployeeId_EffectiveFrom",
                table: "EmployeePositionHistories");

            migrationBuilder.DropIndex(
                name: "IX_EmployeePositionHistories_OldPositionId",
                table: "EmployeePositionHistories");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeGradeHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeGradeHistories");

            migrationBuilder.DropColumn(
                name: "GradeCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EmployeePositionHistories");

            migrationBuilder.DropColumn(
                name: "OldPositionId",
                table: "EmployeePositionHistories");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "EmployeePositionHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EmployeeGradeHistories");

            migrationBuilder.DropColumn(
                name: "OldGradeCode",
                table: "EmployeeGradeHistories");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "EmployeeGradeHistories");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePositionHistories_EmployeeId_EffectiveFrom",
                table: "EmployeePositionHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeGradeHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeGradeHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" });
        }
    }
}
