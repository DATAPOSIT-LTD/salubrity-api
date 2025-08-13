using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormSections_SectionId",
                table: "FormFields");

            migrationBuilder.DropIndex(
                name: "IX_FormFields_FormId",
                table: "FormFields");

            migrationBuilder.DropIndex(
                name: "IX_FormFieldOptions_FormFieldId",
                table: "FormFieldOptions");

            migrationBuilder.AddColumn<int>(
                name: "ExpectedParticipants",
                table: "HealthCamps",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "FormFields",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FieldType",
                table: "FormFields",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConditionalLogicType",
                table: "FormFields",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomErrorMessage",
                table: "FormFields",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FormId1",
                table: "FormFields",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FormSectionId",
                table: "FormFields",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasConditionalLogic",
                table: "FormFields",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LayoutPosition",
                table: "FormFields",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxLength",
                table: "FormFields",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxValue",
                table: "FormFields",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinLength",
                table: "FormFields",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinValue",
                table: "FormFields",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TriggerFieldId",
                table: "FormFields",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TriggerValueOptionId",
                table: "FormFields",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationPattern",
                table: "FormFields",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationType",
                table: "FormFields",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "FormFieldOptions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayText",
                table: "FormFieldOptions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FormFieldOptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "FormFieldOptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_FormId_SectionId_Order",
                table: "FormFields",
                columns: new[] { "FormId", "SectionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_FormId1",
                table: "FormFields",
                column: "FormId1");

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_FormSectionId",
                table: "FormFields",
                column: "FormSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_TriggerFieldId",
                table: "FormFields",
                column: "TriggerFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_TriggerValueOptionId",
                table: "FormFields",
                column: "TriggerValueOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldOptions_FormFieldId_Order",
                table: "FormFieldOptions",
                columns: new[] { "FormFieldId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldOptions_FormFieldId_Value",
                table: "FormFieldOptions",
                columns: new[] { "FormFieldId", "Value" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormFieldOptions_TriggerValueOptionId",
                table: "FormFields",
                column: "TriggerValueOptionId",
                principalTable: "FormFieldOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormFields_TriggerFieldId",
                table: "FormFields",
                column: "TriggerFieldId",
                principalTable: "FormFields",
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
                name: "FK_FormFields_Forms_FormId1",
                table: "FormFields",
                column: "FormId1",
                principalTable: "Forms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormFieldOptions_TriggerValueOptionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormFields_TriggerFieldId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormSections_FormSectionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormSections_SectionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_Forms_FormId1",
                table: "FormFields");

            migrationBuilder.DropIndex(
                name: "IX_FormFields_FormId_SectionId_Order",
                table: "FormFields");

            migrationBuilder.DropIndex(
                name: "IX_FormFields_FormId1",
                table: "FormFields");

            migrationBuilder.DropIndex(
                name: "IX_FormFields_FormSectionId",
                table: "FormFields");

            migrationBuilder.DropIndex(
                name: "IX_FormFields_TriggerFieldId",
                table: "FormFields");

            migrationBuilder.DropIndex(
                name: "IX_FormFields_TriggerValueOptionId",
                table: "FormFields");

            migrationBuilder.DropIndex(
                name: "IX_FormFieldOptions_FormFieldId_Order",
                table: "FormFieldOptions");

            migrationBuilder.DropIndex(
                name: "IX_FormFieldOptions_FormFieldId_Value",
                table: "FormFieldOptions");

            migrationBuilder.DropColumn(
                name: "ExpectedParticipants",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "ConditionalLogicType",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "CustomErrorMessage",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "FormId1",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "FormSectionId",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "HasConditionalLogic",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "LayoutPosition",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "MaxLength",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "MaxValue",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "MinLength",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "MinValue",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "TriggerFieldId",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "TriggerValueOptionId",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "ValidationPattern",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "ValidationType",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FormFieldOptions");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "FormFieldOptions");

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "FormFields",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "FieldType",
                table: "FormFields",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "FormFieldOptions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayText",
                table: "FormFieldOptions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_FormId",
                table: "FormFields",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldOptions_FormFieldId",
                table: "FormFieldOptions",
                column: "FormFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormSections_SectionId",
                table: "FormFields",
                column: "SectionId",
                principalTable: "FormSections",
                principalColumn: "Id");
        }
    }
}
