using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSubcontractorAssignmentToPolymorphic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampServiceAssignments_Services_ServiceId",
                table: "HealthCampServiceAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubcontractorHealthCampAssignments_ServiceCategories_Servic~",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubcontractorHealthCampAssignments_ServiceSubcategories_Ser~",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropIndex(
                name: "IX_SubcontractorHealthCampAssignments_ServiceCategoryId",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropIndex(
                name: "IX_SubcontractorHealthCampAssignments_ServiceSubcategoryId",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropIndex(
                name: "IX_HealthCampServiceAssignments_ServiceId",
                table: "HealthCampServiceAssignments");

            migrationBuilder.DropColumn(
                name: "ServiceCategoryId",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropColumn(
                name: "ServiceSubcategoryId",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "HealthCampServiceAssignments",
                newName: "AssignmentId");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignmentId",
                table: "SubcontractorHealthCampAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "AssignmentType",
                table: "SubcontractorHealthCampAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AssignmentType",
                table: "HealthCampServiceAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReportingMetricMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "boolean"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingMetricMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingMetricMappings_IntakeFormField_FieldId",
                        column: x => x.FieldId,
                        principalTable: "IntakeFormField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportingMetricMappings_FieldId",
                table: "ReportingMetricMappings",
                column: "FieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportingMetricMappings");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropColumn(
                name: "AssignmentType",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropColumn(
                name: "AssignmentType",
                table: "HealthCampServiceAssignments");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "HealthCampServiceAssignments",
                newName: "ServiceId");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceCategoryId",
                table: "SubcontractorHealthCampAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceSubcategoryId",
                table: "SubcontractorHealthCampAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorHealthCampAssignments_ServiceCategoryId",
                table: "SubcontractorHealthCampAssignments",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorHealthCampAssignments_ServiceSubcategoryId",
                table: "SubcontractorHealthCampAssignments",
                column: "ServiceSubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampServiceAssignments_ServiceId",
                table: "HealthCampServiceAssignments",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampServiceAssignments_Services_ServiceId",
                table: "HealthCampServiceAssignments",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubcontractorHealthCampAssignments_ServiceCategories_Servic~",
                table: "SubcontractorHealthCampAssignments",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubcontractorHealthCampAssignments_ServiceSubcategories_Ser~",
                table: "SubcontractorHealthCampAssignments",
                column: "ServiceSubcategoryId",
                principalTable: "ServiceSubcategories",
                principalColumn: "Id");
        }
    }
}
