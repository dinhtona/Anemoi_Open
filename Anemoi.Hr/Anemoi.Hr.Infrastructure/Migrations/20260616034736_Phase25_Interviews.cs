using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Hr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase25_Interviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    InterviewerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Result = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewSchedules_CandidateApplications_CandidateApplicati~",
                        column: x => x.CandidateApplicationId,
                        principalTable: "CandidateApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewSchedules_Employees_InterviewerEmployeeId",
                        column: x => x.InterviewerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterviewFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Strengths = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Concerns = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewFeedbacks_Employees_InterviewerEmployeeId",
                        column: x => x.InterviewerEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewFeedbacks_InterviewSchedules_InterviewScheduleId",
                        column: x => x.InterviewScheduleId,
                        principalTable: "InterviewSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedbacks_InterviewerEmployeeId",
                table: "InterviewFeedbacks",
                column: "InterviewerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedbacks_InterviewScheduleId_InterviewerEmployeeId",
                table: "InterviewFeedbacks",
                columns: new[] { "InterviewScheduleId", "InterviewerEmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_CandidateApplicationId",
                table: "InterviewSchedules",
                column: "CandidateApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_InterviewerEmployeeId",
                table: "InterviewSchedules",
                column: "InterviewerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_Result",
                table: "InterviewSchedules",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_ScheduledAt",
                table: "InterviewSchedules",
                column: "ScheduledAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewFeedbacks");

            migrationBuilder.DropTable(
                name: "InterviewSchedules");
        }
    }
}
