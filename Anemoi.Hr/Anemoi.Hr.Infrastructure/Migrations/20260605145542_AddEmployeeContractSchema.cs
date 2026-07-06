using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeContractSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContractTypeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SignedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    AttachmentFileId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PreviousContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TerminatedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TerminationReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TerminationNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TerminationAttachmentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeContracts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_ContractNumber",
                table: "EmployeeContracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_EmployeeId_StartDate_EndDate",
                table: "EmployeeContracts",
                columns: new[] { "EmployeeId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_EmployeeId_StatusCode",
                table: "EmployeeContracts",
                columns: new[] { "EmployeeId", "StatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_StatusCode_EndDate",
                table: "EmployeeContracts",
                columns: new[] { "StatusCode", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeContracts");
        }
    }
}
