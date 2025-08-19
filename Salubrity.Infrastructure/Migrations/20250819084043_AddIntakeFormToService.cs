using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIntakeFormToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceRange",
                table: "HealthAssessmentMetrics");

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "HealthAssessmentRecommendations",
                type: "integer",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfigId",
                table: "HealthAssessmentRecommendations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HealthMetricStatusId",
                table: "HealthAssessmentRecommendations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MetricConfigId",
                table: "HealthAssessmentRecommendations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "HealthAssessmentRecommendations",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "HealthAssessmentRecommendations",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfigId",
                table: "HealthAssessmentMetrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MetricConfigId",
                table: "HealthAssessmentMetrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetricScoreMethods",
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
                    table.PrimaryKey("PK_MetricScoreMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetricValueTypes",
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
                    table.PrimaryKey("PK_MetricValueTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthMetricConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ValueTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScoreMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    MinAcceptable = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxAcceptable = table.Column<decimal>(type: "numeric", nullable: true),
                    InterpretationFormula = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_HealthMetricConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthMetricConfigs_MetricScoreMethods_ScoreMethodId",
                        column: x => x.ScoreMethodId,
                        principalTable: "MetricScoreMethods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HealthMetricConfigs_MetricValueTypes_ValueTypeId",
                        column: x => x.ValueTypeId,
                        principalTable: "MetricValueTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HealthMetricThresholds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    StatusLabel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HealthMetricThresholds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthMetricThresholds_HealthMetricConfigs_MetricConfigId",
                        column: x => x.MetricConfigId,
                        principalTable: "HealthMetricConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthMetricThresholds_HealthMetricStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "HealthMetricStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentRecommendations_ConfigId",
                table: "HealthAssessmentRecommendations",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentRecommendations_HealthMetricStatusId",
                table: "HealthAssessmentRecommendations",
                column: "HealthMetricStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentMetrics_ConfigId",
                table: "HealthAssessmentMetrics",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthMetricConfigs_ScoreMethodId",
                table: "HealthMetricConfigs",
                column: "ScoreMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthMetricConfigs_ValueTypeId",
                table: "HealthMetricConfigs",
                column: "ValueTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthMetricThresholds_MetricConfigId",
                table: "HealthMetricThresholds",
                column: "MetricConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthMetricThresholds_StatusId",
                table: "HealthMetricThresholds",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessmentMetrics_HealthMetricConfigs_ConfigId",
                table: "HealthAssessmentMetrics",
                column: "ConfigId",
                principalTable: "HealthMetricConfigs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessmentRecommendations_HealthMetricConfigs_ConfigId",
                table: "HealthAssessmentRecommendations",
                column: "ConfigId",
                principalTable: "HealthMetricConfigs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthAssessmentRecommendations_HealthMetricStatus_HealthMe~",
                table: "HealthAssessmentRecommendations",
                column: "HealthMetricStatusId",
                principalTable: "HealthMetricStatus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthAssessmentMetrics_HealthMetricConfigs_ConfigId",
                table: "HealthAssessmentMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthAssessmentRecommendations_HealthMetricConfigs_ConfigId",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthAssessmentRecommendations_HealthMetricStatus_HealthMe~",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropTable(
                name: "HealthMetricThresholds");

            migrationBuilder.DropTable(
                name: "HealthMetricConfigs");

            migrationBuilder.DropTable(
                name: "MetricScoreMethods");

            migrationBuilder.DropTable(
                name: "MetricValueTypes");

            migrationBuilder.DropIndex(
                name: "IX_HealthAssessmentRecommendations_ConfigId",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_HealthAssessmentRecommendations_HealthMetricStatusId",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_HealthAssessmentMetrics_ConfigId",
                table: "HealthAssessmentMetrics");

            migrationBuilder.DropColumn(
                name: "ConfigId",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropColumn(
                name: "HealthMetricStatusId",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropColumn(
                name: "MetricConfigId",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "HealthAssessmentRecommendations");

            migrationBuilder.DropColumn(
                name: "ConfigId",
                table: "HealthAssessmentMetrics");

            migrationBuilder.DropColumn(
                name: "MetricConfigId",
                table: "HealthAssessmentMetrics");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "HealthAssessmentRecommendations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceRange",
                table: "HealthAssessmentMetrics",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
