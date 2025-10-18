using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateHealthCampPackagesEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HealthCampPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCampPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCampPackages_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthCampPackageServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCampPackageServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCampPackageServices_HealthCampPackages_HealthCampPack~",
                        column: x => x.HealthCampPackageId,
                        principalTable: "HealthCampPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampPackages_ServicePackageId",
                table: "HealthCampPackages",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampPackageServices_HealthCampPackageId",
                table: "HealthCampPackageServices",
                column: "HealthCampPackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthCampPackageServices");

            migrationBuilder.DropTable(
                name: "HealthCampPackages");
        }
    }
}
