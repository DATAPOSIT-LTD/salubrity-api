using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingStatusLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BillingStatusId",
                table: "HealthCampParticipants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BillingStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingStatus", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampParticipants_BillingStatusId",
                table: "HealthCampParticipants",
                column: "BillingStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampParticipants_BillingStatus_BillingStatusId",
                table: "HealthCampParticipants",
                column: "BillingStatusId",
                principalTable: "BillingStatus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampParticipants_BillingStatus_BillingStatusId",
                table: "HealthCampParticipants");

            migrationBuilder.DropTable(
                name: "BillingStatus");

            migrationBuilder.DropIndex(
                name: "IX_HealthCampParticipants_BillingStatusId",
                table: "HealthCampParticipants");

            migrationBuilder.DropColumn(
                name: "BillingStatusId",
                table: "HealthCampParticipants");
        }
    }
}
