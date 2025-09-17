using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorRecommendationReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Conclusion",
                table: "DoctorRecommendations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosticImpression",
                table: "DoctorRecommendations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PertinentClinicalFindings",
                table: "DoctorRecommendations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PertinentHistoryFindings",
                table: "DoctorRecommendations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conclusion",
                table: "DoctorRecommendations");

            migrationBuilder.DropColumn(
                name: "DiagnosticImpression",
                table: "DoctorRecommendations");

            migrationBuilder.DropColumn(
                name: "PertinentClinicalFindings",
                table: "DoctorRecommendations");

            migrationBuilder.DropColumn(
                name: "PertinentHistoryFindings",
                table: "DoctorRecommendations");
        }
    }
}
