using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIntakeFormsToServiceLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IntakeFormId",
                table: "ServiceSubcategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IntakeFormId",
                table: "ServiceCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSubcategories_IntakeFormId",
                table: "ServiceSubcategories",
                column: "IntakeFormId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_IntakeFormId",
                table: "ServiceCategories",
                column: "IntakeFormId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCategories_IntakeForms_IntakeFormId",
                table: "ServiceCategories",
                column: "IntakeFormId",
                principalTable: "IntakeForms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceSubcategories_IntakeForms_IntakeFormId",
                table: "ServiceSubcategories",
                column: "IntakeFormId",
                principalTable: "IntakeForms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCategories_IntakeForms_IntakeFormId",
                table: "ServiceCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceSubcategories_IntakeForms_IntakeFormId",
                table: "ServiceSubcategories");

            migrationBuilder.DropIndex(
                name: "IX_ServiceSubcategories_IntakeFormId",
                table: "ServiceSubcategories");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_IntakeFormId",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "IntakeFormId",
                table: "ServiceSubcategories");

            migrationBuilder.DropColumn(
                name: "IntakeFormId",
                table: "ServiceCategories");
        }
    }
}
