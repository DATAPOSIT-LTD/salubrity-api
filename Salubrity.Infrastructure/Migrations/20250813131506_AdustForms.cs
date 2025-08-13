using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdustForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormFieldOptions_TriggerValueOptionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormSections_FormSectionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormSections_SectionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_Forms_FormId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_Forms_FormId1",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormFieldOptions_IntakeFormFields_FieldId",
                table: "IntakeFormFieldOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormFieldResponses_IntakeFormFields_FieldId",
                table: "IntakeFormFieldResponses");

            migrationBuilder.DropTable(
                name: "FormFieldOptions");

            migrationBuilder.DropTable(
                name: "FormSections");

            migrationBuilder.DropTable(
                name: "IntakeFormFields");

            migrationBuilder.DropTable(
                name: "Forms");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormFieldOptions_FieldId",
                table: "IntakeFormFieldOptions");

            migrationBuilder.RenameColumn(
                name: "FormSectionId",
                table: "FormFields",
                newName: "IntakeFormSectionId");

            migrationBuilder.RenameColumn(
                name: "FormId1",
                table: "FormFields",
                newName: "IntakeFormId");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_FormSectionId",
                table: "FormFields",
                newName: "IX_FormFields_IntakeFormSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_FormId1",
                table: "FormFields",
                newName: "IX_FormFields_IntakeFormId");

            migrationBuilder.AddColumn<Guid>(
                name: "IntakeFormId",
                table: "IntakeFormSections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "IntakeForms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "IntakeFormFieldOptions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayText",
                table: "IntakeFormFieldOptions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "IntakeFormFieldOptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "FieldTypeId",
                table: "FormFields",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormSections_IntakeFormId",
                table: "IntakeFormSections",
                column: "IntakeFormId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormFieldOptions_FieldId_Order",
                table: "IntakeFormFieldOptions",
                columns: new[] { "FieldId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormFieldOptions_FieldId_Value",
                table: "IntakeFormFieldOptions",
                columns: new[] { "FieldId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_FieldTypeId",
                table: "FormFields",
                column: "FieldTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FieldTypes_FieldTypeId",
                table: "FormFields",
                column: "FieldTypeId",
                principalTable: "FieldTypes",
                principalColumn: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormSections_IntakeForms_IntakeFormId",
                table: "IntakeFormSections",
                column: "IntakeFormId",
                principalTable: "IntakeForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FieldTypes_FieldTypeId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeFormSections_IntakeForms_IntakeFormId",
                table: "IntakeFormSections");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormSections_IntakeFormId",
                table: "IntakeFormSections");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormFieldOptions_FieldId_Order",
                table: "IntakeFormFieldOptions");

            migrationBuilder.DropIndex(
                name: "IX_IntakeFormFieldOptions_FieldId_Value",
                table: "IntakeFormFieldOptions");

            migrationBuilder.DropIndex(
                name: "IX_FormFields_FieldTypeId",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "IntakeFormId",
                table: "IntakeFormSections");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "DisplayText",
                table: "IntakeFormFieldOptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "IntakeFormFieldOptions");

            migrationBuilder.DropColumn(
                name: "FieldTypeId",
                table: "FormFields");

            migrationBuilder.RenameColumn(
                name: "IntakeFormSectionId",
                table: "FormFields",
                newName: "FormSectionId");

            migrationBuilder.RenameColumn(
                name: "IntakeFormId",
                table: "FormFields",
                newName: "FormId1");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_IntakeFormSectionId",
                table: "FormFields",
                newName: "IX_FormFields_FormSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormFields_IntakeFormId",
                table: "FormFields",
                newName: "IX_FormFields_FormId1");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "IntakeFormFieldOptions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "FormFieldOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFieldOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormFieldOptions_FormFields_FormFieldId",
                        column: x => x.FormFieldId,
                        principalTable: "FormFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntakeFormFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Label = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeFormFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeFormFields_FieldTypes_FieldTypeId",
                        column: x => x.FieldTypeId,
                        principalTable: "FieldTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntakeFormFields_IntakeFormSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "IntakeFormSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    FormId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSections_Forms_FormId1",
                        column: x => x.FormId1,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormFieldOptions_FieldId",
                table: "IntakeFormFieldOptions",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldOptions_FormFieldId_Order",
                table: "FormFieldOptions",
                columns: new[] { "FormFieldId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldOptions_FormFieldId_Value",
                table: "FormFieldOptions",
                columns: new[] { "FormFieldId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormSections_FormId1",
                table: "FormSections",
                column: "FormId1");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormFields_FieldTypeId",
                table: "IntakeFormFields",
                column: "FieldTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormFields_SectionId",
                table: "IntakeFormFields",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormFieldOptions_TriggerValueOptionId",
                table: "FormFields",
                column: "TriggerValueOptionId",
                principalTable: "FormFieldOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormSections_FormSectionId",
                table: "FormFields",
                column: "FormSectionId",
                principalTable: "FormSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormSections_SectionId",
                table: "FormFields",
                column: "SectionId",
                principalTable: "FormSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_Forms_FormId",
                table: "FormFields",
                column: "FormId",
                principalTable: "Forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_Forms_FormId1",
                table: "FormFields",
                column: "FormId1",
                principalTable: "Forms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormFieldOptions_IntakeFormFields_FieldId",
                table: "IntakeFormFieldOptions",
                column: "FieldId",
                principalTable: "IntakeFormFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeFormFieldResponses_IntakeFormFields_FieldId",
                table: "IntakeFormFieldResponses",
                column: "FieldId",
                principalTable: "IntakeFormFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
