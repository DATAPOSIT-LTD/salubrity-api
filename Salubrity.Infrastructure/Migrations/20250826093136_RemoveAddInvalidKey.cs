using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAddInvalidKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthAssessmentFormResponses_HealthAssessments_HealthAsses~",
                table: "HealthAssessmentFormResponses");

            migrationBuilder.DropIndex(
                name: "IX_HealthAssessmentFormResponses_HealthAssessmentId",
                table: "HealthAssessmentFormResponses");

            migrationBuilder.DropColumn(
                name: "HealthAssessmentId",
                table: "HealthAssessmentFormResponses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HealthAssessmentId",
                table: "HealthAssessmentFormResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentFormResponses_HealthAssessmentId",
                table: "HealthAssessmentFormResponses",
                column: "HealthAssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessmentFormResponses_HealthAssessments_HealthAsses~",
                table: "HealthAssessmentFormResponses",
                column: "HealthAssessmentId",
                principalTable: "HealthAssessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
