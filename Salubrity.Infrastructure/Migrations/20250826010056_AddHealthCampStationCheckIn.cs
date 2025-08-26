using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthCampStationCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HealthCampStationCheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampServiceAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HealthCampStationCheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCampStationCheckIns_HealthCampParticipants_HealthCamp~",
                        column: x => x.HealthCampParticipantId,
                        principalTable: "HealthCampParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthCampStationCheckIns_HealthCampServiceAssignments_Heal~",
                        column: x => x.HealthCampServiceAssignmentId,
                        principalTable: "HealthCampServiceAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampStationCheckIns_HealthCampParticipantId",
                table: "HealthCampStationCheckIns",
                column: "HealthCampParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampStationCheckIns_HealthCampServiceAssignmentId",
                table: "HealthCampStationCheckIns",
                column: "HealthCampServiceAssignmentId");

            // Index to enforce only one active station at a time per participant
            migrationBuilder.Sql(@"
CREATE UNIQUE INDEX IF NOT EXISTS ux_active_station_per_participant
ON ""HealthCampStationCheckIns"" (""HealthCampId"", ""HealthCampParticipantId"")
WHERE ""Status"" IN ('Queued','InService');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthCampStationCheckIns");
        }
    }
}
