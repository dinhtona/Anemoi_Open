using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentTransferHistoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EmployeeDepartmentHistories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OldDepartmentId",
                table: "EmployeeDepartmentHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentHistories_OldDepartmentId",
                table: "EmployeeDepartmentHistories",
                column: "OldDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDepartmentHistories_Departments_OldDepartmentId",
                table: "EmployeeDepartmentHistories",
                column: "OldDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                INSERT INTO ""EmployeeDepartmentHistories"" (""Id"", ""EmployeeId"", ""DepartmentId"", ""OldDepartmentId"", ""IsPrimary"", ""EffectiveFrom"", ""EffectiveTo"", ""ReasonCode"", ""CreatedAt"", ""CreatedBy"")
                SELECT 
                    gen_random_uuid(), 
                    ""Id"", 
                    ""PrimaryDepartmentId"", 
                    NULL, 
                    TRUE, 
                    ""JoinDate"", 
                    NULL, 
                    'initial_placement', 
                    NOW(), 
                    'system:migration'
                FROM ""Employees""
                WHERE ""PrimaryDepartmentId"" IS NOT NULL
                  AND ""Id"" NOT IN (SELECT DISTINCT ""EmployeeId"" FROM ""EmployeeDepartmentHistories"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDepartmentHistories_Departments_OldDepartmentId",
                table: "EmployeeDepartmentHistories");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDepartmentHistories_OldDepartmentId",
                table: "EmployeeDepartmentHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EmployeeDepartmentHistories");

            migrationBuilder.DropColumn(
                name: "OldDepartmentId",
                table: "EmployeeDepartmentHistories");
        }
    }
}
