using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class formsections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormSection_SectionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormSection_Forms_FormId1",
                table: "FormSection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FormSection",
                table: "FormSection");

            migrationBuilder.RenameTable(
                name: "FormSection",
                newName: "FormSections");

            migrationBuilder.RenameIndex(
                name: "IX_FormSection_FormId1",
                table: "FormSections",
                newName: "IX_FormSections_FormId1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FormSections",
                table: "FormSections",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormSections_SectionId",
                table: "FormFields",
                column: "SectionId",
                principalTable: "FormSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormSections_Forms_FormId1",
                table: "FormSections",
                column: "FormId1",
                principalTable: "Forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormFields_FormSections_SectionId",
                table: "FormFields");

            migrationBuilder.DropForeignKey(
                name: "FK_FormSections_Forms_FormId1",
                table: "FormSections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FormSections",
                table: "FormSections");

            migrationBuilder.RenameTable(
                name: "FormSections",
                newName: "FormSection");

            migrationBuilder.RenameIndex(
                name: "IX_FormSections_FormId1",
                table: "FormSection",
                newName: "IX_FormSection_FormId1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FormSection",
                table: "FormSection",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormFields_FormSection_SectionId",
                table: "FormFields",
                column: "SectionId",
                principalTable: "FormSection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormSection_Forms_FormId1",
                table: "FormSection",
                column: "FormId1",
                principalTable: "Forms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
