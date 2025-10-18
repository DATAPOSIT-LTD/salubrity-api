using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthCampPackageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampPackage_HealthCamps_HealthCampId",
                table: "HealthCampPackage");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampPackage_ServicePackages_ServicePackageId",
                table: "HealthCampPackage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HealthCampPackage",
                table: "HealthCampPackage");

            migrationBuilder.RenameTable(
                name: "HealthCampPackage",
                newName: "HealthCampPackages");

            migrationBuilder.RenameIndex(
                name: "IX_HealthCampPackage_ServicePackageId",
                table: "HealthCampPackages",
                newName: "IX_HealthCampPackages_ServicePackageId");

            migrationBuilder.RenameIndex(
                name: "IX_HealthCampPackage_HealthCampId",
                table: "HealthCampPackages",
                newName: "IX_HealthCampPackages_HealthCampId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HealthCampPackages",
                table: "HealthCampPackages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampPackages_HealthCamps_HealthCampId",
                table: "HealthCampPackages",
                column: "HealthCampId",
                principalTable: "HealthCamps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampPackages_ServicePackages_ServicePackageId",
                table: "HealthCampPackages",
                column: "ServicePackageId",
                principalTable: "ServicePackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampPackages_HealthCamps_HealthCampId",
                table: "HealthCampPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampPackages_ServicePackages_ServicePackageId",
                table: "HealthCampPackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HealthCampPackages",
                table: "HealthCampPackages");

            migrationBuilder.RenameTable(
                name: "HealthCampPackages",
                newName: "HealthCampPackage");

            migrationBuilder.RenameIndex(
                name: "IX_HealthCampPackages_ServicePackageId",
                table: "HealthCampPackage",
                newName: "IX_HealthCampPackage_ServicePackageId");

            migrationBuilder.RenameIndex(
                name: "IX_HealthCampPackages_HealthCampId",
                table: "HealthCampPackage",
                newName: "IX_HealthCampPackage_HealthCampId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HealthCampPackage",
                table: "HealthCampPackage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampPackage_HealthCamps_HealthCampId",
                table: "HealthCampPackage",
                column: "HealthCampId",
                principalTable: "HealthCamps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampPackage_ServicePackages_ServicePackageId",
                table: "HealthCampPackage",
                column: "ServicePackageId",
                principalTable: "ServicePackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
