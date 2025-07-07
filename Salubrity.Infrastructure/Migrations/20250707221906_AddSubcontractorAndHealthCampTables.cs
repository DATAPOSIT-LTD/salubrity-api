using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubcontractorAndHealthCampTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Camps");

            migrationBuilder.DropTable(
                name: "ServicePackageItems");

            migrationBuilder.CreateTable(
                name: "HealthCamps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HealthCamps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCamps_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationPackage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_OrganizationPackage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationPackage_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationPackage_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubcontractorHealthCampAssignmentStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_SubcontractorHealthCampAssignmentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubcontractorStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_SubcontractorStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthCampPackageItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceType = table.Column<int>(type: "integer", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HealthCampPackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCampPackageItems_HealthCamps_HealthCampId",
                        column: x => x.HealthCampId,
                        principalTable: "HealthCamps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthCampPackageItems_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HealthCampServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HealthCampServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCampServices_HealthCamps_HealthCampId",
                        column: x => x.HealthCampId,
                        principalTable: "HealthCamps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthCampServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subcontractors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndustryId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_Subcontractors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subcontractors_Industries_IndustryId",
                        column: x => x.IndustryId,
                        principalTable: "Industries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subcontractors_SubcontractorStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "SubcontractorStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subcontractors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthCampServiceAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubcontractorId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HealthCampServiceAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCampServiceAssignments_HealthCamps_HealthCampId",
                        column: x => x.HealthCampId,
                        principalTable: "HealthCamps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthCampServiceAssignments_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthCampServiceAssignments_Subcontractors_SubcontractorId",
                        column: x => x.SubcontractorId,
                        principalTable: "Subcontractors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubcontractorHealthCampAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthCampId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubcontractorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceSubcategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    BoothLabel = table.Column<string>(type: "text", nullable: false),
                    RoomNumber = table.Column<string>(type: "text", nullable: true),
                    AssignmentStatusId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_SubcontractorHealthCampAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubcontractorHealthCampAssignments_HealthCamps_HealthCampId",
                        column: x => x.HealthCampId,
                        principalTable: "HealthCamps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubcontractorHealthCampAssignments_ServiceCategories_Servic~",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubcontractorHealthCampAssignments_ServiceSubcategories_Ser~",
                        column: x => x.ServiceSubcategoryId,
                        principalTable: "ServiceSubcategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubcontractorHealthCampAssignments_SubcontractorHealthCampA~",
                        column: x => x.AssignmentStatusId,
                        principalTable: "SubcontractorHealthCampAssignmentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubcontractorHealthCampAssignments_Subcontractors_Subcontra~",
                        column: x => x.SubcontractorId,
                        principalTable: "Subcontractors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubcontractorRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubcontractorId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_SubcontractorRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubcontractorRoles_Subcontractors_SubcontractorId",
                        column: x => x.SubcontractorId,
                        principalTable: "Subcontractors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubcontractorSpecialties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubcontractorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_SubcontractorSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubcontractorSpecialties_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubcontractorSpecialties_Subcontractors_SubcontractorId",
                        column: x => x.SubcontractorId,
                        principalTable: "Subcontractors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubcontractorRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubcontractorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubcontractorRoleId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_SubcontractorRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubcontractorRoleAssignments_SubcontractorRoles_Subcontract~",
                        column: x => x.SubcontractorRoleId,
                        principalTable: "SubcontractorRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubcontractorRoleAssignments_Subcontractors_SubcontractorId",
                        column: x => x.SubcontractorId,
                        principalTable: "Subcontractors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampPackageItems_HealthCampId",
                table: "HealthCampPackageItems",
                column: "HealthCampId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampPackageItems_ServicePackageId",
                table: "HealthCampPackageItems",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCamps_OrganizationId",
                table: "HealthCamps",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampServiceAssignments_HealthCampId",
                table: "HealthCampServiceAssignments",
                column: "HealthCampId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampServiceAssignments_ServiceId",
                table: "HealthCampServiceAssignments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampServiceAssignments_SubcontractorId",
                table: "HealthCampServiceAssignments",
                column: "SubcontractorId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampServices_HealthCampId",
                table: "HealthCampServices",
                column: "HealthCampId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCampServices_ServiceId",
                table: "HealthCampServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPackage_OrganizationId",
                table: "OrganizationPackage",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPackage_ServicePackageId",
                table: "OrganizationPackage",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorHealthCampAssignments_AssignmentStatusId",
                table: "SubcontractorHealthCampAssignments",
                column: "AssignmentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorHealthCampAssignments_HealthCampId",
                table: "SubcontractorHealthCampAssignments",
                column: "HealthCampId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorHealthCampAssignments_ServiceCategoryId",
                table: "SubcontractorHealthCampAssignments",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorHealthCampAssignments_ServiceSubcategoryId",
                table: "SubcontractorHealthCampAssignments",
                column: "ServiceSubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorHealthCampAssignments_SubcontractorId",
                table: "SubcontractorHealthCampAssignments",
                column: "SubcontractorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorRoleAssignments_SubcontractorId",
                table: "SubcontractorRoleAssignments",
                column: "SubcontractorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorRoleAssignments_SubcontractorRoleId",
                table: "SubcontractorRoleAssignments",
                column: "SubcontractorRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorRoles_SubcontractorId",
                table: "SubcontractorRoles",
                column: "SubcontractorId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_IndustryId",
                table: "Subcontractors",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_StatusId",
                table: "Subcontractors",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_UserId",
                table: "Subcontractors",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorSpecialties_ServiceId",
                table: "SubcontractorSpecialties",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorSpecialties_SubcontractorId",
                table: "SubcontractorSpecialties",
                column: "SubcontractorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthCampPackageItems");

            migrationBuilder.DropTable(
                name: "HealthCampServiceAssignments");

            migrationBuilder.DropTable(
                name: "HealthCampServices");

            migrationBuilder.DropTable(
                name: "OrganizationPackage");

            migrationBuilder.DropTable(
                name: "SubcontractorHealthCampAssignments");

            migrationBuilder.DropTable(
                name: "SubcontractorRoleAssignments");

            migrationBuilder.DropTable(
                name: "SubcontractorSpecialties");

            migrationBuilder.DropTable(
                name: "HealthCamps");

            migrationBuilder.DropTable(
                name: "SubcontractorHealthCampAssignmentStatuses");

            migrationBuilder.DropTable(
                name: "SubcontractorRoles");

            migrationBuilder.DropTable(
                name: "Subcontractors");

            migrationBuilder.DropTable(
                name: "SubcontractorStatuses");

            migrationBuilder.CreateTable(
                name: "Camps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackageItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicePackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceType = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePackageItems_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackageItems_ServicePackageId",
                table: "ServicePackageItems",
                column: "ServicePackageId");
        }
    }
}
