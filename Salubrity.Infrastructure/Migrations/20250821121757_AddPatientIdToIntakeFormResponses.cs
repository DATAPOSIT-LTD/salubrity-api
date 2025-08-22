using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIdToIntakeFormResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FieldTypes_FieldTypeId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormFields_TriggerFieldId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_IntakeFormFieldOptions_TriggerValueOptionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_IntakeFormSections_IntakeFormSectionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_IntakeFormSections_SectionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_IntakeForms_FormId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_IntakeForms_IntakeFormId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormFieldOptions_FormFields_FieldId",
                table: "IntakeFormFieldOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormFieldResponses_FormFields_FieldId",
                table: "IntakeFormFieldResponses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FormFields",
                table: "FormFields");

            migrationBuilder.RenameTable(
                name: "FormFields",
                newName: "IntakeFormField");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_TriggerValueOptionId",
                table: "IntakeFormField",
                newName: "IX_IntakeFormField_TriggerValueOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_TriggerFieldId",
                table: "IntakeFormField",
                newName: "IX_IntakeFormField_TriggerFieldId");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_SectionId",
                table: "IntakeFormField",
                newName: "IX_IntakeFormField_SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_IntakeFormSectionId",
                table: "IntakeFormField",
                newName: "IX_IntakeFormField_IntakeFormSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_IntakeFormId",
                table: "IntakeFormField",
                newName: "IX_IntakeFormField_IntakeFormId");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_FormId_SectionId_Order",
                table: "IntakeFormField",
                newName: "IX_IntakeFormField_FormId_SectionId_Order");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_FieldTypeId",
                table: "IntakeFormField",
                newName: "IX_IntakeFormField_FieldTypeId");

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "IntakeFormResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_IntakeFormField",
                table: "IntakeFormField",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_PatientId",
                table: "IntakeFormResponses",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormField_FieldTypes_FieldTypeId",
                table: "IntakeFormField",
                column: "FieldTypeId",
                principalTable: "FieldTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormField_IntakeFormFieldOptions_TriggerValueOptionId",
                table: "IntakeFormField",
                column: "TriggerValueOptionId",
                principalTable: "IntakeFormFieldOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormField_IntakeFormField_TriggerFieldId",
                table: "IntakeFormField",
                column: "TriggerFieldId",
                principalTable: "IntakeFormField",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormField_IntakeFormSections_IntakeFormSectionId",
                table: "IntakeFormField",
                column: "IntakeFormSectionId",
                principalTable: "IntakeFormSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormField_IntakeFormSections_SectionId",
                table: "IntakeFormField",
                column: "SectionId",
                principalTable: "IntakeFormSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormField_IntakeForms_FormId",
                table: "IntakeFormField",
                column: "FormId",
                principalTable: "IntakeForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormField_IntakeForms_IntakeFormId",
                table: "IntakeFormField",
                column: "IntakeFormId",
                principalTable: "IntakeForms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormFieldOptions_IntakeFormField_FieldId",
                table: "IntakeFormFieldOptions",
                column: "FieldId",
                principalTable: "IntakeFormField",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormFieldResponses_IntakeFormField_FieldId",
                table: "IntakeFormFieldResponses",
                column: "FieldId",
                principalTable: "IntakeFormField",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormResponses_Patients_PatientId",
                table: "IntakeFormResponses",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormField_FieldTypes_FieldTypeId",
                table: "IntakeFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormField_IntakeFormFieldOptions_TriggerValueOptionId",
                table: "IntakeFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormField_IntakeFormField_TriggerFieldId",
                table: "IntakeFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormField_IntakeFormSections_IntakeFormSectionId",
                table: "IntakeFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormField_IntakeFormSections_SectionId",
                table: "IntakeFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormField_IntakeForms_FormId",
                table: "IntakeFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormField_IntakeForms_IntakeFormId",
                table: "IntakeFormField");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormFieldOptions_IntakeFormField_FieldId",
                table: "IntakeFormFieldOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormFieldResponses_IntakeFormField_FieldId",
                table: "IntakeFormFieldResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormResponses_Patients_PatientId",
                table: "IntakeFormResponses");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormResponses_PatientId",
                table: "IntakeFormResponses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IntakeFormField",
                table: "IntakeFormField");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "IntakeFormResponses");

            migrationBuilder.RenameTable(
                name: "IntakeFormField",
                newName: "FormFields");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeFormField_TriggerValueOptionId",
                table: "FormFields",
                newName: "IX_FormFields_TriggerValueOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeFormField_TriggerFieldId",
                table: "FormFields",
                newName: "IX_FormFields_TriggerFieldId");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeFormField_SectionId",
                table: "FormFields",
                newName: "IX_FormFields_SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeFormField_IntakeFormSectionId",
                table: "FormFields",
                newName: "IX_FormFields_IntakeFormSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeFormField_IntakeFormId",
                table: "FormFields",
                newName: "IX_FormFields_IntakeFormId");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeFormField_FormId_SectionId_Order",
                table: "FormFields",
                newName: "IX_FormFields_FormId_SectionId_Order");

            migrationBuilder.RenameIndex(
                name: "IX_IntakeFormField_FieldTypeId",
                table: "FormFields",
                newName: "IX_FormFields_FieldTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FormFields",
                table: "FormFields",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FieldTypes_FieldTypeId",
                table: "FormFields",
                column: "FieldTypeId",
                principalTable: "FieldTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormFields_TriggerFieldId",
                table: "FormFields",
                column: "TriggerFieldId",
                principalTable: "FormFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_IntakeFormFieldOptions_TriggerValueOptionId",
                table: "FormFields",
                column: "TriggerValueOptionId",
                principalTable: "IntakeFormFieldOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_IntakeFormSections_IntakeFormSectionId",
                table: "FormFields",
                column: "IntakeFormSectionId",
                principalTable: "IntakeFormSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_IntakeFormSections_SectionId",
                table: "FormFields",
                column: "SectionId",
                principalTable: "IntakeFormSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_IntakeForms_FormId",
                table: "FormFields",
                column: "FormId",
                principalTable: "IntakeForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_IntakeForms_IntakeFormId",
                table: "FormFields",
                column: "IntakeFormId",
                principalTable: "IntakeForms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormFieldOptions_FormFields_FieldId",
                table: "IntakeFormFieldOptions",
                column: "FieldId",
                principalTable: "FormFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormFieldResponses_FormFields_FieldId",
                table: "IntakeFormFieldResponses",
                column: "FieldId",
                principalTable: "FormFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
