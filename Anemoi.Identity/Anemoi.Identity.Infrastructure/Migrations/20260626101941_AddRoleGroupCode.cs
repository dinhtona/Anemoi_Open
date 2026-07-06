using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleGroupCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "RoleGroups",
                type: "text",
                nullable: true);

            // Populate Code for existing role groups using Name as basis
            // This matches what SanitizeCode does in IdentityMapper
            // Only lowercase alphanumeric characters, replacing spaces/special chars with empty
            // This is a simplified version that just lowercases and removes non-alphanumeric chars
            migrationBuilder.Sql(
                @"UPDATE ""RoleGroups"" SET ""Code"" = regexp_replace(lower(""Name""), '[^a-z0-9]', '', 'g') WHERE ""Code"" IS NULL;");

            // Make Code non-nullable after population
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "RoleGroups",
                type: "text",
                nullable: false,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "RoleGroups");
        }
    }
}
