using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecreateIntakeFormResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormResponses_Services_ServiceId",
                table: "IntakeFormResponses");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormResponses_ServiceId",
                table: "IntakeFormResponses");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "IntakeFormResponses");

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedServiceId",
                table: "IntakeFormResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedServiceId",
                table: "IntakeFormResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "SubmittedServiceType",
                table: "IntakeFormResponses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_ResolvedServiceId",
                table: "IntakeFormResponses",
                column: "ResolvedServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormResponses_Services_ResolvedServiceId",
                table: "IntakeFormResponses",
                column: "ResolvedServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormResponses_Services_ResolvedServiceId",
                table: "IntakeFormResponses");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormResponses_ResolvedServiceId",
                table: "IntakeFormResponses");

            migrationBuilder.DropColumn(
                name: "ResolvedServiceId",
                table: "IntakeFormResponses");

            migrationBuilder.DropColumn(
                name: "SubmittedServiceId",
                table: "IntakeFormResponses");

            migrationBuilder.DropColumn(
                name: "SubmittedServiceType",
                table: "IntakeFormResponses");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "IntakeFormResponses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_ServiceId",
                table: "IntakeFormResponses",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormResponses_Services_ServiceId",
                table: "IntakeFormResponses",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }
    }
}