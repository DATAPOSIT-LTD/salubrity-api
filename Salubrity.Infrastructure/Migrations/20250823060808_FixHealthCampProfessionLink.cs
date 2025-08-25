using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixHealthCampProfessionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_HealthCampServiceAssignments_ProfessionId",
                table: "HealthCampServiceAssignments",
                column: "ProfessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampServiceAssignments_SubcontractorRoles_ProfessionId",
                table: "HealthCampServiceAssignments",
                column: "ProfessionId",
                principalTable: "SubcontractorRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampServiceAssignments_SubcontractorRoles_ProfessionId",
                table: "HealthCampServiceAssignments");

            migrationBuilder.DropIndex(
                name: "IX_HealthCampServiceAssignments_ProfessionId",
                table: "HealthCampServiceAssignments");
        }
    }
}
