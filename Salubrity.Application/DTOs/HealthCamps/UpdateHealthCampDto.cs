using System;
using System.Collections.Generic;

namespace Salubrity.Application.DTOs.HealthCamps
{
    public class UpdateHealthCampDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public bool? IsActive { get; set; }
        public int? ExpectedParticipants { get; set; }
        public Guid? OrganizationId { get; set; }

        // 🔥 New unified multi-package structure (same as Create DTO)
        public List<UpdateCampPackageDto> Packages { get; set; } = new();
    }

    // ───────────────────────────────────────────────
    // Represents a single package block in an update
    // ───────────────────────────────────────────────
    public class UpdateCampPackageDto
    {
        public Guid PackageId { get; set; }                   // Reference to ServicePackage
        public string? DisplayName { get; set; }              // Optional camp-level override
        public decimal? PriceOverride { get; set; }           // Optional custom pricing
        public bool? IsActive { get; set; } = true;           // Keep/deactivate flag

        public List<HealthCampPackageItemDto> PackageItems { get; set; } = new();
        public List<HealthCampServiceAssignmentDto> ServiceAssignments { get; set; } = new();
    }
}
