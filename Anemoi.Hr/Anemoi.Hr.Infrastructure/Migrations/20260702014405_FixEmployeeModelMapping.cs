using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEmployeeModelMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_DirectManagerEmployeeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DirectManagerEmployeeId",
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
                name: "FK_Employees_Employees_DirectManagerId",
                table: "Employees",
                column: "DirectManagerId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_DirectManagerId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_DirectManagerEmployeeId",
                table: "Employees",
                column: "DirectManagerEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
