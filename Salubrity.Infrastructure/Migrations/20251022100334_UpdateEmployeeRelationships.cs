using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthCampPackageServices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_HealthCampPackageServices_HealthCampPackageId",
                table: "HealthCampPackageServices",
                column: "HealthCampPackageId");
        }
    }
}
