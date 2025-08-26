using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthAssessmentFormSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HealthAssessmentFormTypes",
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
                    table.PrimaryKey("PK_HealthAssessmentFormTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthAssessmentFormResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeFormVersionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HealthAssessmentFormResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthAssessmentFormResponses_HealthAssessmentFormTypes_For~",
                        column: x => x.FormTypeId,
                        principalTable: "HealthAssessmentFormTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthAssessmentFormResponses_HealthAssessments_HealthAsses~",
                        column: x => x.HealthAssessmentId,
                        principalTable: "HealthAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthAssessmentFormResponses_IntakeFormVersions_IntakeForm~",
                        column: x => x.IntakeFormVersionId,
                        principalTable: "IntakeFormVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HealthAssessmentDynamicFieldResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    SelectedOptionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HealthAssessmentDynamicFieldResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthAssessmentDynamicFieldResponses_HealthAssessmentFormR~",
                        column: x => x.FormResponseId,
                        principalTable: "HealthAssessmentFormResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthAssessmentDynamicFieldResponses_IntakeFormField_Field~",
                        column: x => x.FieldId,
                        principalTable: "IntakeFormField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentDynamicFieldResponses_FieldId",
                table: "HealthAssessmentDynamicFieldResponses",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentDynamicFieldResponses_FormResponseId",
                table: "HealthAssessmentDynamicFieldResponses",
                column: "FormResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentFormResponses_FormTypeId",
                table: "HealthAssessmentFormResponses",
                column: "FormTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentFormResponses_HealthAssessmentId",
                table: "HealthAssessmentFormResponses",
                column: "HealthAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentFormResponses_IntakeFormVersionId",
                table: "HealthAssessmentFormResponses",
                column: "IntakeFormVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthAssessmentDynamicFieldResponses");

            migrationBuilder.DropTable(
                name: "HealthAssessmentFormResponses");

            migrationBuilder.DropTable(
                name: "HealthAssessmentFormTypes");
        }
    }
}
