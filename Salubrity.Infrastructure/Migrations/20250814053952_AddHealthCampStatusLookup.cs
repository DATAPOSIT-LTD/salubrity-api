using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthCampStatusLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HealthCampStatusId",
                table: "HealthCamps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HealthCampStatuses",
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
                    table.PrimaryKey("PK_HealthCampStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCamps_HealthCampStatusId",
                table: "HealthCamps",
                column: "HealthCampStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCamps_HealthCampStatuses_HealthCampStatusId",
                table: "HealthCamps",
                column: "HealthCampStatusId",
                principalTable: "HealthCampStatuses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCamps_HealthCampStatuses_HealthCampStatusId",
                table: "HealthCamps");

            migrationBuilder.DropTable(
                name: "HealthCampStatuses");

            migrationBuilder.DropIndex(
                name: "IX_HealthCamps_HealthCampStatusId",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "HealthCampStatusId",
                table: "HealthCamps");
        }
    }
}
