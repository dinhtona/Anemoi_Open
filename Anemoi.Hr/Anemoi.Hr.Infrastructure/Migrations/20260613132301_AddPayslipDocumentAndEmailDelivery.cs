using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayslipDocumentAndEmailDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayslipDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayslipId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ChecksumHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GeneratedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayslipDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayslipDocuments_Payslips_PayslipId",
                        column: x => x.PayslipId,
                        principalTable: "Payslips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayslipEmailDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayslipId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayslipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SentBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayslipEmailDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayslipEmailDeliveries_PayslipDocuments_PayslipDocumentId",
                        column: x => x.PayslipDocumentId,
                        principalTable: "PayslipDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayslipEmailDeliveries_Payslips_PayslipId",
                        column: x => x.PayslipId,
                        principalTable: "Payslips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayslipDocuments_PayslipId",
                table: "PayslipDocuments",
                column: "PayslipId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipDocuments_PayslipId_IsActive",
                table: "PayslipDocuments",
                columns: new[] { "PayslipId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PayslipEmailDeliveries_PayslipDocumentId",
                table: "PayslipEmailDeliveries",
                column: "PayslipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipEmailDeliveries_PayslipId",
                table: "PayslipEmailDeliveries",
                column: "PayslipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayslipEmailDeliveries");

            migrationBuilder.DropTable(
                name: "PayslipDocuments");
        }
    }
}
