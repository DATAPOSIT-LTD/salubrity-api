using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServicePackageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServicePackageId",
                table: "HealthCamps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthCamps_ServicePackageId",
                table: "HealthCamps",
                column: "ServicePackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCamps_ServicePackages_ServicePackageId",
                table: "HealthCamps",
                column: "ServicePackageId",
                principalTable: "ServicePackages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCamps_ServicePackages_ServicePackageId",
                table: "HealthCamps");

            migrationBuilder.DropIndex(
                name: "IX_HealthCamps_ServicePackageId",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "ServicePackageId",
                table: "HealthCamps");
        }
    }
}
