using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthCampPackageIdToParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HealthCampPackageId",
                table: "HealthCampParticipants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampParticipants_HealthCampPackageId",
                table: "HealthCampParticipants",
                column: "HealthCampPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampParticipants_HealthCampPackages_HealthCampPackage~",
                table: "HealthCampParticipants",
                column: "HealthCampPackageId",
                principalTable: "HealthCampPackages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampParticipants_HealthCampPackages_HealthCampPackage~",
                table: "HealthCampParticipants");

            migrationBuilder.DropIndex(
                name: "IX_HealthCampParticipants_HealthCampPackageId",
                table: "HealthCampParticipants");

            migrationBuilder.DropColumn(
                name: "HealthCampPackageId",
                table: "HealthCampParticipants");
        }
    }
}
