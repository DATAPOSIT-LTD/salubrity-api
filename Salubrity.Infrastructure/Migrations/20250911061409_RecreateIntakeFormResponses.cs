using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecreateIntakeFormResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeFormResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeFormVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedServiceType = table.Column<int>(type: "integer", nullable: false),
                    ResolvedServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseStatusId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_IntakeFormResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeFormResponses_IntakeFormVersions_IntakeFormVersionId",
                        column: x => x.IntakeFormVersionId,
                        principalTable: "IntakeFormVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntakeFormResponses_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntakeFormResponses_Services_ResolvedServiceId",
                        column: x => x.ResolvedServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntakeFormResponses_IntakeFormResponseStatuses_ResponseStatusId",
                        column: x => x.ResponseStatusId,
                        principalTable: "IntakeFormResponseStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_IntakeFormVersionId",
                table: "IntakeFormResponses",
                column: "IntakeFormVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_PatientId",
                table: "IntakeFormResponses",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_ResolvedServiceId",
                table: "IntakeFormResponses",
                column: "ResolvedServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeFormResponses_ResponseStatusId",
                table: "IntakeFormResponses",
                column: "ResponseStatusId");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
               migrationBuilder.DropTable(name: "IntakeFormResponses");
        }
    }
}
