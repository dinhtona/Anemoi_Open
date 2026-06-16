using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase25_Applications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentStage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateApplications_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateApplications_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CandidateApplicationStageHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ToStage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateApplicationStageHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateApplicationStageHistories_CandidateApplications_Ca~",
                        column: x => x.CandidateApplicationId,
                        principalTable: "CandidateApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_AppliedAt",
                table: "CandidateApplications",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_CandidateId",
                table: "CandidateApplications",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_CandidateId_JobPostingId",
                table: "CandidateApplications",
                columns: new[] { "CandidateId", "JobPostingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_CurrentStage",
                table: "CandidateApplications",
                column: "CurrentStage");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_JobPostingId",
                table: "CandidateApplications",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplicationStageHistories_CandidateApplicationId",
                table: "CandidateApplicationStageHistories",
                column: "CandidateApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateApplicationStageHistories");

            migrationBuilder.DropTable(
                name: "CandidateApplications");
        }
    }
}
