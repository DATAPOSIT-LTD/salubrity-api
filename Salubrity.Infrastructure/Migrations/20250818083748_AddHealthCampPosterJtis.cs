using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthCampPosterJtis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TempPasswordExpiresAt",
                table: "SubcontractorHealthCampAssignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TempPasswordHash",
                table: "SubcontractorHealthCampAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDate",
                table: "HealthCamps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLaunched",
                table: "HealthCamps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LaunchedAt",
                table: "HealthCamps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParticipantPosterJti",
                table: "HealthCamps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PosterTokensExpireAt",
                table: "HealthCamps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubcontractorPosterJti",
                table: "HealthCamps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HealthMetricStatusId",
                table: "HealthCampParticipants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TempPasswordExpiresAt",
                table: "HealthCampParticipants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TempPasswordHash",
                table: "HealthCampParticipants",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HealthAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HealthAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthAssessments_Employees_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HealthAssessments_HealthCampParticipants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "HealthCampParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HealthAssessments_HealthCamps_HealthCampId",
                        column: x => x.HealthCampId,
                        principalTable: "HealthCamps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthCampTempCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TempPasswordHash = table.Column<string>(type: "text", nullable: true),
                    TempPasswordExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SignInJti = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TokenExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HealthCampTempCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCampTempCredentials_HealthCamps_HealthCampId",
                        column: x => x.HealthCampId,
                        principalTable: "HealthCamps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthCampTempCredentials_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthMetricStatus",
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
                    table.PrimaryKey("PK_HealthMetricStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthAssessmentRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Priority = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("PK_HealthAssessmentRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthAssessmentRecommendations_HealthAssessments_HealthAss~",
                        column: x => x.HealthAssessmentId,
                        principalTable: "HealthAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthAssessmentMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: true),
                    ReferenceRange = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HealthMetricStatusId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HealthAssessmentMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthAssessmentMetrics_HealthAssessments_HealthAssessmentId",
                        column: x => x.HealthAssessmentId,
                        principalTable: "HealthAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthAssessmentMetrics_HealthMetricStatus_HealthMetricStat~",
                        column: x => x.HealthMetricStatusId,
                        principalTable: "HealthMetricStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampParticipants_HealthMetricStatusId",
                table: "HealthCampParticipants",
                column: "HealthMetricStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentMetrics_HealthAssessmentId",
                table: "HealthAssessmentMetrics",
                column: "HealthAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentMetrics_HealthMetricStatusId",
                table: "HealthAssessmentMetrics",
                column: "HealthMetricStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessmentRecommendations_HealthAssessmentId",
                table: "HealthAssessmentRecommendations",
                column: "HealthAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_HealthCampId",
                table: "HealthAssessments",
                column: "HealthCampId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_ParticipantId",
                table: "HealthAssessments",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthAssessments_ReviewedById",
                table: "HealthAssessments",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampTempCredentials_HealthCampId",
                table: "HealthCampTempCredentials",
                column: "HealthCampId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampTempCredentials_UserId",
                table: "HealthCampTempCredentials",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCampParticipants_HealthMetricStatus_HealthMetricStatu~",
                table: "HealthCampParticipants",
                column: "HealthMetricStatusId",
                principalTable: "HealthMetricStatus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCampParticipants_HealthMetricStatus_HealthMetricStatu~",
                table: "HealthCampParticipants");

            migrationBuilder.DropTable(
                name: "HealthAssessmentMetrics");

            migrationBuilder.DropTable(
                name: "HealthAssessmentRecommendations");

            migrationBuilder.DropTable(
                name: "HealthCampTempCredentials");

            migrationBuilder.DropTable(
                name: "HealthMetricStatus");

            migrationBuilder.DropTable(
                name: "HealthAssessments");

            migrationBuilder.DropIndex(
                name: "IX_HealthCampParticipants_HealthMetricStatusId",
                table: "HealthCampParticipants");

            migrationBuilder.DropColumn(
                name: "TempPasswordExpiresAt",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropColumn(
                name: "TempPasswordHash",
                table: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "IsLaunched",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "LaunchedAt",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "ParticipantPosterJti",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "PosterTokensExpireAt",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "SubcontractorPosterJti",
                table: "HealthCamps");

            migrationBuilder.DropColumn(
                name: "HealthMetricStatusId",
                table: "HealthCampParticipants");

            migrationBuilder.DropColumn(
                name: "TempPasswordExpiresAt",
                table: "HealthCampParticipants");

            migrationBuilder.DropColumn(
                name: "TempPasswordHash",
                table: "HealthCampParticipants");
        }
    }
}
