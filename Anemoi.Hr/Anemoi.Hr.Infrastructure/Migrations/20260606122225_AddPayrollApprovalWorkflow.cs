using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "PayrollRuns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "PayrollRuns",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "PayrollRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "PayrollRuns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "PayrollRuns",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAt",
                table: "PayrollRuns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalizedBy",
                table: "PayrollRuns",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "PayrollRuns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "PayrollRuns",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "PayrollRuns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PayrollRuns",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "PayrollRuns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedBy",
                table: "PayrollRuns",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "FinalizedBy",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "PayrollRuns");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "PayrollRuns");
        }
    }
}
