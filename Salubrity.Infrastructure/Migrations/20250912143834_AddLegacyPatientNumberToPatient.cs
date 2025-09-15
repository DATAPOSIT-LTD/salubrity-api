using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyPatientNumberToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegacyPatientNumber",
                table: "Patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientNumber",
                table: "Patients",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormFieldMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeFormVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeFormFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Alias = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFieldMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormFieldMappings_IntakeFormField_IntakeFormFieldId",
                        column: x => x.IntakeFormFieldId,
                        principalTable: "IntakeFormField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormFieldMappings_IntakeFormVersions_IntakeFormVersionId",
                        column: x => x.IntakeFormVersionId,
                        principalTable: "IntakeFormVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldMappings_IntakeFormFieldId",
                table: "FormFieldMappings",
                column: "IntakeFormFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldMappings_IntakeFormVersionId_Alias",
                table: "FormFieldMappings",
                columns: new[] { "IntakeFormVersionId", "Alias" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormFieldMappings");

            migrationBuilder.DropColumn(
                name: "LegacyPatientNumber",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PatientNumber",
                table: "Patients");
        }
    }
}
