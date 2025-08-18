// File: Salubrity.Application/DTOs/HealthCamps/LaunchDtos.cs
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.HealthCamps;

public sealed class LaunchHealthCampDto
{
    [Required] public Guid HealthCampId { get; set; }
    [Required] public DateTime CloseDate { get; set; } // UTC
}

