using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase25_HiringDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HiringDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DecidedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiringDecisions_CandidateApplications_CandidateApplicationId",
                        column: x => x.CandidateApplicationId,
                        principalTable: "CandidateApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_CandidateApplicationId",
                table: "HiringDecisions",
                column: "CandidateApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_DecidedAt",
                table: "HiringDecisions",
                column: "DecidedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_DecidedBy",
                table: "HiringDecisions",
                column: "DecidedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_Decision",
                table: "HiringDecisions",
                column: "Decision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiringDecisions");
        }
    }
}
