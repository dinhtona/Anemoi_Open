using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEmployeeMappingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_PrimaryDepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_DirectManagerId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Positions_PrimaryPositionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DirectManagerId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DirectManagerId",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DirectManagerEmployeeId",
                table: "Employees",
                column: "DirectManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "ix_employees_identity_user_id",
                table: "Employees",
                column: "IdentityUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employees_work_email",
                table: "Employees",
                column: "WorkEmail",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_PrimaryDepartmentId",
                table: "Employees",
                column: "PrimaryDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_DirectManagerEmployeeId",
                table: "Employees",
                column: "DirectManagerEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Positions_PrimaryPositionId",
                table: "Employees",
                column: "PrimaryPositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_PrimaryDepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_DirectManagerEmployeeId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Positions_PrimaryPositionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DirectManagerEmployeeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "ix_employees_identity_user_id",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "ix_employees_work_email",
                table: "Employees");

            migrationBuilder.AddColumn<Guid>(
                name: "DirectManagerId",
                table: "Employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DirectManagerId",
                table: "Employees",
                column: "DirectManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_PrimaryDepartmentId",
                table: "Employees",
                column: "PrimaryDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_DirectManagerId",
                table: "Employees",
                column: "DirectManagerId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Positions_PrimaryPositionId",
                table: "Employees",
                column: "PrimaryPositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
