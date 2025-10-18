using System;

namespace Salubrity.Application.DTOs.HealthCamps
{
    public class HealthCampPackageDto
    {
        public Guid Id { get; set; }
        public Guid HealthCampId { get; set; }
        public Guid ServicePackageId { get; set; }
        public string? ServicePackageName { get; set; }
    }
}
