using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeForHealthAssessmentResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_ServiceId",
                table: "IntakeFormResponses",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormResponses_Services_ServiceId",
                table: "IntakeFormResponses",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormResponses_Services_ServiceId",
                table: "IntakeFormResponses");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormResponses_ServiceId",
                table: "IntakeFormResponses");
        }
    }
}
