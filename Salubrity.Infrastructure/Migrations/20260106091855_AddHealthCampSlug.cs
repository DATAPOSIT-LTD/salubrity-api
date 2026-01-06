using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    public partial class AddHealthCampSlug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add column as nullable
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "HealthCamps",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            // 2. Backfill slugs safely (collision-proof)
            migrationBuilder.Sql(@"
        WITH base_slugs AS (
            SELECT
                ""Id"",
                LOWER(
                    REGEXP_REPLACE(
                        REGEXP_REPLACE(
                            REGEXP_REPLACE(TRIM(""Name""), '[^a-zA-Z0-9\s-]', '', 'g'),
                            '\s+', '-', 'g'
                        ),
                        '-{2,}', '-', 'g'
                    )
                )
                || '-' || EXTRACT(YEAR FROM ""StartDate"")::text AS base_slug
            FROM ""HealthCamps""
        ),
        numbered AS (
            SELECT
                ""Id"",
                base_slug,
                ROW_NUMBER() OVER (PARTITION BY base_slug ORDER BY ""Id"") AS rn
            FROM base_slugs
        )
        UPDATE ""HealthCamps"" hc
        SET ""Slug"" =
            CASE
                WHEN n.rn = 1 THEN n.base_slug
                ELSE n.base_slug || '-' || n.rn
            END
        FROM numbered n
        WHERE hc.""Id"" = n.""Id""
          AND hc.""Slug"" IS NULL;
    ");

            // 3. Enforce NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "HealthCamps",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            // 4. Enforce uniqueness
            migrationBuilder.CreateIndex(
                name: "IX_HealthCamps_Slug",
                table: "HealthCamps",
                column: "Slug",
                unique: true);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HealthCamps_Slug",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "HealthCamps");
        }
    }
}
