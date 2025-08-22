using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SubcontractorAccessEndpointsAuthGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProfessionId",
                table: "HealthCampServiceAssignments",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfessionId",
                table: "HealthCampServiceAssignments");
        }
    }
}
