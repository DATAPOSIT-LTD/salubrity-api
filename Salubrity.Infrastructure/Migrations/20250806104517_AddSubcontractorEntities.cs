using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubcontractorEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubcontractorRoles_Subcontractors_SubcontractorId",
                table: "SubcontractorRoles");

            migrationBuilder.DropIndex(
                name: "IX_SubcontractorRoles_SubcontractorId",
                table: "SubcontractorRoles");

            migrationBuilder.DropColumn(
                name: "SubcontractorId",
                table: "SubcontractorRoles");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "SubcontractorRoleAssignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "SubcontractorRoleAssignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SubcontractorRoleAssignments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "SubcontractorRoleAssignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoomNumber",
                table: "SubcontractorHealthCampAssignments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BoothLabel",
                table: "SubcontractorHealthCampAssignments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "SubcontractorHealthCampAssignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryAssignment",
                table: "SubcontractorHealthCampAssignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SubcontractorHealthCampAssignments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "SubcontractorHealthCampAssignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "SubcontractorRoleAssignments");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "SubcontractorRoleAssignments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SubcontractorRoleAssignments");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "SubcontractorRoleAssignments");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropColumn(
                name: "IsPrimaryAssignment",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.AddColumn<Guid>(
                name: "SubcontractorId",
                table: "SubcontractorRoles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoomNumber",
                table: "SubcontractorHealthCampAssignments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BoothLabel",
                table: "SubcontractorHealthCampAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorRoles_SubcontractorId",
                table: "SubcontractorRoles",
                column: "SubcontractorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubcontractorRoles_Subcontractors_SubcontractorId",
                table: "SubcontractorRoles",
                column: "SubcontractorId",
                principalTable: "Subcontractors",
                principalColumn: "Id");
        }
    }
}
