using Anemoi.Identity.Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anemoi.Identity.Infrastructure.Migrations;

[DbContext(typeof(IdentityDbContext))]
[Migration("20260530000000_AddIdentityPolicyUniqueIndexes")]
public partial class AddIdentityPolicyUniqueIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            WITH ranked_policies AS (
                SELECT "Id",
                       FIRST_VALUE("Id") OVER (PARTITION BY "Key", "Value" ORDER BY "Id") AS canonical_id,
                       ROW_NUMBER() OVER (PARTITION BY "Key", "Value" ORDER BY "Id") AS row_number
                FROM "IdentityPolicies"
            )
            UPDATE "IdentityPolicyMapRoles" AS map
            SET "IdentityPolicyId" = ranked.canonical_id
            FROM ranked_policies AS ranked
            WHERE map."IdentityPolicyId" = ranked."Id"
              AND ranked.row_number > 1;

            WITH ranked_policies AS (
                SELECT "Id",
                       ROW_NUMBER() OVER (PARTITION BY "Key", "Value" ORDER BY "Id") AS row_number
                FROM "IdentityPolicies"
            )
            DELETE FROM "IdentityPolicies" AS policy
            USING ranked_policies AS ranked
            WHERE policy."Id" = ranked."Id"
              AND ranked.row_number > 1;

            WITH ranked_maps AS (
                SELECT "Id",
                       ROW_NUMBER() OVER (
                           PARTITION BY "IdentityPolicyId", "UserRoleId"
                           ORDER BY "Id"
                       ) AS row_number
                FROM "IdentityPolicyMapRoles"
            )
            DELETE FROM "IdentityPolicyMapRoles" AS map
            USING ranked_maps AS ranked
            WHERE map."Id" = ranked."Id"
              AND ranked.row_number > 1;
            """);

        migrationBuilder.DropIndex(
            name: "IX_IdentityPolicyMapRoles_IdentityPolicyId",
            table: "IdentityPolicyMapRoles");

        migrationBuilder.CreateIndex(
            name: "IX_IdentityPolicies_Key_Value",
            table: "IdentityPolicies",
            columns: new[] { "Key", "Value" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_IdentityPolicyMapRoles_IdentityPolicyId_UserRoleId",
            table: "IdentityPolicyMapRoles",
            columns: new[] { "IdentityPolicyId", "UserRoleId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_IdentityPolicies_Key_Value",
            table: "IdentityPolicies");

        migrationBuilder.DropIndex(
            name: "IX_IdentityPolicyMapRoles_IdentityPolicyId_UserRoleId",
            table: "IdentityPolicyMapRoles");

        migrationBuilder.CreateIndex(
            name: "IX_IdentityPolicyMapRoles_IdentityPolicyId",
            table: "IdentityPolicyMapRoles",
            column: "IdentityPolicyId");
    }
}
