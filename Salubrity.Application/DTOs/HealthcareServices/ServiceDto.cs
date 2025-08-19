// File: Salubrity.Application/DTOs/HealthcareServices/ServiceDtos.cs

#nullable enable
using System.ComponentModel.DataAnnotations;
using Salubrity.Application.DTOs.Forms;

namespace Salubrity.Application.DTOs.HealthcareServices
{
    // ---------- READ DTOS (full tree) ----------

    public class ServiceResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? PricePerPerson { get; set; }
        public Guid? IndustryId { get; set; }
        public Guid? IntakeFormId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }

        public List<ServiceCategoryDto> Categories { get; set; } = [];
        public FormResponseDto? IntakeForm { get; set; } // 👈 Add this
    }

    public class ServiceCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;

        public List<ServiceSubcategoryDto> Subcategories { get; set; } = [];
    }

    public class ServiceSubcategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ---------- WRITE DTOS (nested upsert) ----------

    public class CreateServiceDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? PricePerPerson { get; set; }

        public Guid? IndustryId { get; set; }
        public Guid? IntakeFormId { get; set; }

        [MaxLength(2048)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateServiceCategoryDto> Categories { get; set; } = [];
    }

    public class CreateServiceCategoryDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public int? DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateServiceSubcategoryDto> Subcategories { get; set; } = [];
        public Guid ServiceId { get; set; }
    }

    public class CreateServiceSubcategoryDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public int? DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateServiceDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? PricePerPerson { get; set; }

        public Guid? IndustryId { get; set; }
        public Guid? IntakeFormId { get; set; }

        [MaxLength(2048)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // Replace semantics: the provided Categories list becomes the source of truth.
        public List<UpdateServiceCategoryDto> Categories { get; set; } = [];
    }

    public class UpdateServiceCategoryDto
    {
        // If Id is null/empty -> create; if present -> update; categories not present -> delete
        public Guid? Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public int? DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;

        public List<UpdateServiceSubcategoryDto> Subcategories { get; set; } = [];
    }

    public class UpdateServiceSubcategoryDto
    {
        // If Id is null/empty -> create; if present -> update; subcats not present -> delete
        public Guid? Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public int? DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
