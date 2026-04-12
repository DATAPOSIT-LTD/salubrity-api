using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthCampIdToIntakeFormResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HealthCampId",
                table: "IntakeFormResponses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_HealthCampId",
                table: "IntakeFormResponses",
                column: "HealthCampId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormResponses_HealthCamps_HealthCampId",
                table: "IntakeFormResponses",
                column: "HealthCampId",
                principalTable: "HealthCamps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormResponses_HealthCamps_HealthCampId",
                table: "IntakeFormResponses");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormResponses_HealthCampId",
                table: "IntakeFormResponses");

            migrationBuilder.DropColumn(
                name: "HealthCampId",
                table: "IntakeFormResponses");
        }
    }
}
