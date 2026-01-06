using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    public partial class AddHealthCampSlug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column as NULLABLE
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "HealthCamps",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            // Backfill slug from Name + year(StartDate)
            migrationBuilder.Sql(@"
                UPDATE ""HealthCamps""
                SET ""Slug"" =
                    LOWER(
                        REGEXP_REPLACE(
                            REGEXP_REPLACE(
                                REGEXP_REPLACE(TRIM(""Name""), '[^a-zA-Z0-9\s-]', '', 'g'),
                                '\s+', '-', 'g'
                            ),
                            '-{2,}', '-', 'g'
                        )
                    )
                    || '-' || EXTRACT(YEAR FROM ""StartDate"")::text
                WHERE ""Slug"" IS NULL;
            ");

            // Enforce NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "HealthCamps",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            // Optional but STRONGLY recommended: unique index
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
